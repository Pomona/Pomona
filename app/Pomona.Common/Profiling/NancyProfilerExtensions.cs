#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;

using Nancy;

using Pomona.Common;

using StackExchange.Profiling;

using HttpUtility = System.Web.HttpUtility;

namespace Bizi.Storefront.Pomona.Profiling
{
    public static class NancyProfilerExtensions
    {
        public static void ProfileStepBegin(this NancyContext context, string name)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (name == null)
                throw new ArgumentNullException("name");
            var profiler = Profiler.Current;
            if (profiler == null)
                return;

            var profile = profiler.Step(name);
            context.Items["ProfileTiming." + name] = profile;
        }


        public static void ProfileStepEnd(this NancyContext context, string name)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (name == null)
                throw new ArgumentNullException("name");
            if (Profiler.Current == null)
                return;
            IDisposable profile;
            var itemKey = "ProfileTiming." + name;
            if (context.Items.TryGetValueAsType(itemKey, out profile))
            {
                profile.Dispose();
                context.Items[itemKey] = null;
            }
        }


        public static void InjectProfilingToBody(this NancyContext ctx, MiniProfiler profiler)
        {
            var streamWriteAction = ctx.Response.Contents;
            var ct = new ContentType(ctx.Response.ContentType);
            if (ct.MediaType == "application/json")
            {
                ctx.Response.Contents = stream =>
                {
                    using (var strWriter = new NoCloseStreamWriter(stream))
                    {
                        foreach (
                            var line in
                                RenderProfilingReport(profiler, false).Replace("\r", "").Split(new char[] { '\n' }))
                        {
                            strWriter.Write("// ");
                            strWriter.WriteLine(line);
                        }
                        strWriter.Flush();
                    }
                    streamWriteAction(stream);
                };
            }
            if (ct.MediaType == "text/html")
            {
                ctx.Response.Contents = stream =>
                {
                    string contentString;
                    using (var memStream = new MemoryStream())
                    {
                        streamWriteAction(memStream);
                        memStream.Flush();
                        contentString = Encoding.UTF8.GetString(memStream.ToArray());
                    }
                    var bodyEndTag = "</body>";
                    var indexOfBodyEnd = contentString.IndexOf(bodyEndTag);
                    contentString = contentString.Insert(indexOfBodyEnd,
                                                         string.Format("<pre>{0}</pre>",
                                                                       RenderProfilingReport(profiler, true)));
                    using (var strWriter = new NoCloseStreamWriter(stream))
                    {
                        strWriter.Write(contentString);
                    }
                };
            }
        }


        private static string RenderProfilingReport(MiniProfiler profiler, bool htmlEncode)
        {
            if (profiler == null)
                return string.Empty;
            StringBuilder stringBuilder =
                new StringBuilder().Append(htmlEncode
                                               ? HttpUtility.HtmlEncode(Environment.MachineName)
                                               : Environment.MachineName).Append(" at ").Append((object)DateTime.UtcNow)
                    .AppendLine();
            Stack<Timing> stack = new Stack<Timing>();
            stack.Push(profiler.Root);
            while (stack.Count > 0)
            {
                Timing timing = stack.Pop();
                string str = htmlEncode ? HttpUtility.HtmlEncode(timing.Name) : timing.Name;
                stringBuilder.AppendFormat("{2} {0} = {1:###,##0.##}ms @ ({3}ms - {4}ms)  | self: {5}ms", (object)str,
                                           (object)timing.DurationMilliseconds,
                                           (object)new string('>', (int)timing.Depth), timing.StartMilliseconds,
                                           timing.StartMilliseconds + timing.DurationMilliseconds,
                                           timing.DurationWithoutChildrenMilliseconds);
                if (timing.HasCustomTimings)
                {
                    foreach (KeyValuePair<string, List<CustomTiming>> keyValuePair in timing.CustomTimings)
                    {
                        string key = keyValuePair.Key;
                        List<CustomTiming> list = keyValuePair.Value;
                        stringBuilder.AppendFormat(" ({0} = {1:###,##0.##}ms in {2} cmd{3})", (object)key,
                                                   (object)
                                                   Enumerable.Sum<CustomTiming>((IEnumerable<CustomTiming>)list,
                                                                                (Func<CustomTiming, Decimal?>)
                                                                                (ct => ct.DurationMilliseconds)),
                                                   (object)list.Count, list.Count == 1 ? (object)"" : (object)"s");
                    }
                }
                stringBuilder.AppendLine();
                if (timing.HasChildren)
                {
                    List<Timing> children = timing.Children;
                    for (int index = children.Count - 1; index >= 0; --index)
                        stack.Push(children[index]);
                }
            }
            return ((object)stringBuilder).ToString();
        }

        #region Nested type: NoCloseStreamWriter

        private class NoCloseStreamWriter : StreamWriter
        {
            public NoCloseStreamWriter(Stream stream)
                : base(stream)
            {
            }


            protected override void Dispose(bool disposing)
            {
                base.Dispose(false);
            }
        }

        #endregion
    }
}