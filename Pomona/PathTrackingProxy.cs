#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Pomona.TestModel;

namespace Pomona
{
    public class PathTrackingListProxy : IList<object>
    {
        private readonly string path;
        private readonly IList<EntityBase> wrappedList;


        public PathTrackingListProxy(IEnumerable<EntityBase> wrappedList, string path)
        {
            this.wrappedList = wrappedList.ToList();
            this.path = path;
        }


        public object this[int index]
        {
            get { return Wrap(this.wrappedList[index]); }
            set { throw new NotImplementedException(); }
        }


        public int Count
        {
            get { return this.wrappedList.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }


        public void Add(object item)
        {
            ThrowReadonlyException();
        }


        public void Clear()
        {
            ThrowReadonlyException();
        }


        public bool Contains(object item)
        {
            throw new NotImplementedException();
        }


        public void CopyTo(object[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        public IEnumerator<object> GetEnumerator()
        {
            return this.wrappedList.Select(Wrap).GetEnumerator();
        }


        public int IndexOf(object item)
        {
            throw new NotImplementedException();
        }


        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }


        public bool Remove(object item)
        {
            ThrowReadonlyException();
            return false;
        }


        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }


        private static void ThrowReadonlyException()
        {
            throw new NotSupportedException("Wrapping list is read-only");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private object Wrap(EntityBase entity)
        {
            return new PathTrackingProxy(entity, this.path);
        }
    }

    public class PathTrackingProxy : DynamicObject
    {
        private static readonly string[] extraProperties = new string[] { "_path" };
        private readonly string path;
        private readonly HashSet<string> expandedPaths;
        private readonly EntityBase wrappedObject;
        private readonly Type wrappedType;

        public PathTrackingProxy(EntityBase wrappedObject, string path)
        {
            throw new NotImplementedException();
        }


        public PathTrackingProxy(EntityBase wrappedObject, string path, HashSet<string> expandedPaths)
        {
            this.wrappedType = wrappedObject.GetType();
            this.wrappedObject = wrappedObject;
            this.path = path;
            this.expandedPaths = expandedPaths;
        }


        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.wrappedObject.GetType().GetProperties().Select(x => x.Name).Concat(extraProperties);
        }

        private static string GetUriForEntity(EntityBase entity)
        {
            return string.Format(
                    "http://localhost:2222/{0}/{1}", entity.GetType().Name.ToLower(), entity.Id);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "_path")
            {
                result = this.path;
                return true;
            }

            result = null;

            var propInfo = this.wrappedObject.GetType().GetProperty(binder.Name);
            if (propInfo == null)
                return false;

            result = propInfo.GetValue(this.wrappedObject, null);

            EntityBase entityResult;
            var subPath = this.path + "." + binder.Name;

            if ((entityResult = result as EntityBase) != null)
            {

                result = GetExpandedOrReference(entityResult, subPath);
            }

            if (IsIList(result))
            {
                result =
                    new List<object>(
                        ((IEnumerable<EntityBase>)result).Select(x => GetExpandedOrReference(x, subPath)).Cast<object>().
                            ToList());
                //result = new PathTrackingListProxy((IEnumerable<EntityBase>)result, subPath);
            }

            return true;
            //return base.TryGetMember(binder, out result);
        }


        private object GetExpandedOrReference(EntityBase entityResult, string subPath)
        {
            object result;
            if (!this.expandedPaths.Contains(subPath))
                result = new { _uri = GetUriForEntity(entityResult) };
            else
                result = new PathTrackingProxy(entityResult, subPath, expandedPaths);
            return result;
        }


        private bool IsIList(object obj)
        {
            return
                obj.GetType().GetInterfaces().Any(
                    x => x.IsGenericType &&
                    x.GetGenericTypeDefinition() == typeof(IList<>)
                    && typeof(EntityBase).IsAssignableFrom(x.GetGenericArguments()[0]));
        }
    }
}