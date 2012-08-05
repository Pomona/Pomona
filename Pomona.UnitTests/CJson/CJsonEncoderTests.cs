// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pomona.Sandbox.CJson;

namespace Pomona.UnitTests.CJson
{
    [TestFixture]
    public class CJsonEncoderTests
    {
        private static void Parse(string jsonToParse)
        {
            var cjsonEncoder = new CJsonEncoder();
            cjsonEncoder.Parse(jsonToParse);

            Console.WriteLine("Compressed size: " + cjsonEncoder.SizeCompressed);
            Console.WriteLine("Uncompressed size: " + cjsonEncoder.SizeNotCompressed);
        }

        private int GetZippedLength(byte[] array)
        {
            var zmemStream = new MemoryStream();

            int zcompressedLength;

            using (var zstream = new GZipStream(zmemStream, CompressionMode.Compress))
            {
                zstream.Write(array, 0, array.Length);
                zstream.Flush();
            }

            return (int) zmemStream.ToArray().Length;
        }

        private string bigJsonFileWithCritters =
            @"[
  {
    ""_uri"": ""http://localhost:2211/critter/77"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Distinct Budgerigar has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/77/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/76"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 76
    },
    ""id"": 77,
    ""name"": ""Distinct Budgerigar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/74"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/77""
        },
        ""id"": 74,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        },
        ""sku"": ""2715"",
        ""startsOn"": ""2012-08-19T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/75"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/77""
        },
        ""id"": 75,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        },
        ""sku"": ""2208"",
        ""startsOn"": ""2012-10-20T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/70"",
        ""dependability"": 0.01949272771342319,
        ""id"": 70,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/71"",
        ""_type"": ""Gun"",
        ""dependability"": 0.85390793152801125,
        ""explosionFactor"": 0.18986279805650133,
        ""id"": 71,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/72"",
        ""_type"": ""Gun"",
        ""dependability"": 0.54473644287546,
        ""explosionFactor"": 0.050817372301042718,
        ""id"": 72,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/73"",
        ""dependability"": 0.48024091612558856,
        ""id"": 73,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/81"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Gaseous Seal has acne""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/81/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/80"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 80
    },
    ""id"": 81,
    ""name"": ""Gaseous Seal"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/79"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/81""
        },
        ""id"": 79,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""5453"",
        ""startsOn"": ""2012-11-22T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/78"",
        ""_type"": ""Gun"",
        ""dependability"": 0.41854639417424166,
        ""explosionFactor"": 0.35643959294838812,
        ""id"": 78,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/89"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Responsible Sloth has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/89/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/88"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 88
    },
    ""id"": 89,
    ""name"": ""Responsible Sloth"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/85"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/89""
        },
        ""id"": 85,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""6622"",
        ""startsOn"": ""2012-08-10T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/86"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/89""
        },
        ""id"": 86,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        },
        ""sku"": ""6518"",
        ""startsOn"": ""2012-10-22T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/87"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/89""
        },
        ""id"": 87,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        },
        ""sku"": ""3846"",
        ""startsOn"": ""2012-08-27T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/82"",
        ""_type"": ""Gun"",
        ""dependability"": 0.18679526736344923,
        ""explosionFactor"": 0.43386475715500522,
        ""id"": 82,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/83"",
        ""_type"": ""Gun"",
        ""dependability"": 0.36775914131093729,
        ""explosionFactor"": 0.45646231363362738,
        ""id"": 83,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/84"",
        ""dependability"": 0.5483650823814632,
        ""id"": 84,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/92"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Elementary Aardvark has adenoma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/92/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/91"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 91
    },
    ""id"": 92,
    ""name"": ""Elementary Aardvark"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/90"",
        ""dependability"": 0.89077329490835466,
        ""id"": 90,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/98"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Fixed Ewe has adenoma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/98/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/97"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 97
    },
    ""id"": 98,
    ""name"": ""Fixed Ewe"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/94"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/98""
        },
        ""id"": 94,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""8895"",
        ""startsOn"": ""2012-10-04T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/95"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/98""
        },
        ""id"": 95,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        },
        ""sku"": ""3523"",
        ""startsOn"": ""2012-11-08T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/96"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/98""
        },
        ""id"": 96,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""5206"",
        ""startsOn"": ""2012-09-17T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/93"",
        ""dependability"": 0.9438327103591676,
        ""id"": 93,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/104"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Poised Pig has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/104/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/103"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 103
    },
    ""id"": 104,
    ""name"": ""Poised Pig"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/100"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/104""
        },
        ""id"": 100,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        },
        ""sku"": ""8820"",
        ""startsOn"": ""2012-08-07T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/101"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/104""
        },
        ""id"": 101,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        },
        ""sku"": ""5656"",
        ""startsOn"": ""2012-11-17T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/102"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/104""
        },
        ""id"": 102,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""2483"",
        ""startsOn"": ""2012-10-13T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/99"",
        ""_type"": ""Gun"",
        ""dependability"": 0.17562022673693495,
        ""explosionFactor"": 0.12927481258719917,
        ""id"": 99,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/110"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Anchored Mare has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/110/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/109"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 109
    },
    ""id"": 110,
    ""name"": ""Anchored Mare"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/108"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/110""
        },
        ""id"": 108,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""sku"": ""5591"",
        ""startsOn"": ""2012-09-23T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/105"",
        ""_type"": ""Gun"",
        ""dependability"": 0.49502573325067095,
        ""explosionFactor"": 0.68480016090199358,
        ""id"": 105,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/106"",
        ""dependability"": 0.3633674370885675,
        ""id"": 106,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/107"",
        ""dependability"": 0.5148508234484358,
        ""id"": 107,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/117"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Humiliating Oryx has dehydration""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/117/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/116"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 116
    },
    ""id"": 117,
    ""name"": ""Humiliating Oryx"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/115"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/117""
        },
        ""id"": 115,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        },
        ""sku"": ""1015"",
        ""startsOn"": ""2012-10-05T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/111"",
        ""_type"": ""Gun"",
        ""dependability"": 0.97016622310977718,
        ""explosionFactor"": 0.18469185344162017,
        ""id"": 111,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/112"",
        ""_type"": ""Gun"",
        ""dependability"": 0.48711420990857957,
        ""explosionFactor"": 0.14734574321068159,
        ""id"": 112,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/113"",
        ""dependability"": 0.28000297829508919,
        ""id"": 113,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/114"",
        ""_type"": ""Gun"",
        ""dependability"": 0.99892503675023325,
        ""explosionFactor"": 0.926447535365097,
        ""id"": 114,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/124"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Optimistic Jaguar has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/124/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/123"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 123
    },
    ""id"": 124,
    ""name"": ""Optimistic Jaguar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/120"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/124""
        },
        ""id"": 120,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        },
        ""sku"": ""708"",
        ""startsOn"": ""2012-08-15T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/121"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/124""
        },
        ""id"": 121,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        },
        ""sku"": ""9062"",
        ""startsOn"": ""2012-08-22T08:17:25.8216081Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/122"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/124""
        },
        ""id"": 122,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""6746"",
        ""startsOn"": ""2012-09-07T08:17:25.8216081Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/118"",
        ""_type"": ""Gun"",
        ""dependability"": 0.40708276741536464,
        ""explosionFactor"": 0.38962483331077957,
        ""id"": 118,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/119"",
        ""dependability"": 0.95636456783691637,
        ""id"": 119,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/131"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Lost Polar has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/131/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/130"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 130
    },
    ""id"": 131,
    ""name"": ""Lost Polar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/128"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/131""
        },
        ""id"": 128,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        },
        ""sku"": ""3911"",
        ""startsOn"": ""2012-10-07T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/129"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/131""
        },
        ""id"": 129,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""4265"",
        ""startsOn"": ""2012-10-15T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/125"",
        ""_type"": ""Gun"",
        ""dependability"": 0.63277222338727312,
        ""explosionFactor"": 0.21292520277804006,
        ""id"": 125,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/126"",
        ""dependability"": 0.8166109155009551,
        ""id"": 126,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/127"",
        ""_type"": ""Gun"",
        ""dependability"": 0.98032631817289,
        ""explosionFactor"": 0.70188546772202731,
        ""id"": 127,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/138"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tangible Fish has thrush""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/138/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/137"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 137
    },
    ""id"": 138,
    ""instrument"": ""Instructive Drum"",
    ""name"": ""Tangible Fish"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/134"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/138"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 134,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""9402"",
        ""startsOn"": ""2012-08-25T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/135"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/138"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 135,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        },
        ""sku"": ""5078"",
        ""startsOn"": ""2012-08-26T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/136"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/138"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 136,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        },
        ""sku"": ""240"",
        ""startsOn"": ""2012-08-03T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/132"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90878824419751214,
        ""explosionFactor"": 0.98397313802688058,
        ""id"": 132,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/133"",
        ""dependability"": 0.98963369940856183,
        ""id"": 133,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/143"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Stylish Civet has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/143/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/142"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 142
    },
    ""id"": 143,
    ""name"": ""Stylish Civet"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/139"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25477706280293738,
        ""explosionFactor"": 0.073970737435841344,
        ""id"": 139,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/140"",
        ""dependability"": 0.87493311514842,
        ""id"": 140,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/141"",
        ""dependability"": 0.32379278509122916,
        ""id"": 141,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/147"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Outstanding Tapir has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/147/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/146"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 146
    },
    ""id"": 147,
    ""instrument"": ""Gentle Metallophone"",
    ""name"": ""Outstanding Tapir"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/145"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/147"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 145,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        },
        ""sku"": ""3406"",
        ""startsOn"": ""2012-11-14T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/144"",
        ""dependability"": 0.665452947684309,
        ""id"": 144,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/152"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Illustrious Hog has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/152/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/151"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 151
    },
    ""id"": 152,
    ""name"": ""Illustrious Hog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/148"",
        ""dependability"": 0.14087041613686382,
        ""id"": 148,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/149"",
        ""_type"": ""Gun"",
        ""dependability"": 0.83717011280226061,
        ""explosionFactor"": 0.72919607522394325,
        ""id"": 149,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/150"",
        ""dependability"": 0.29994862121527482,
        ""id"": 150,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/159"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Variable Civet has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/159/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/158"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 158
    },
    ""id"": 159,
    ""name"": ""Variable Civet"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/156"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/159""
        },
        ""id"": 156,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""1552"",
        ""startsOn"": ""2012-10-13T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/157"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/159""
        },
        ""id"": 157,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        },
        ""sku"": ""4595"",
        ""startsOn"": ""2012-08-26T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/153"",
        ""dependability"": 0.1162001924199053,
        ""id"": 153,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/154"",
        ""_type"": ""Gun"",
        ""dependability"": 0.050703755603499594,
        ""explosionFactor"": 0.10344130038444013,
        ""id"": 154,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/32""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/155"",
        ""dependability"": 0.24510950559988129,
        ""id"": 155,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/165"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Polished Ocelot has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/165/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/164"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 164
    },
    ""id"": 165,
    ""name"": ""Polished Ocelot"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/163"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/165""
        },
        ""id"": 163,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        },
        ""sku"": ""2991"",
        ""startsOn"": ""2012-10-19T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/160"",
        ""_type"": ""Gun"",
        ""dependability"": 0.87159222125615565,
        ""explosionFactor"": 0.59282539672815493,
        ""id"": 160,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/161"",
        ""_type"": ""Gun"",
        ""dependability"": 0.61013411386410432,
        ""explosionFactor"": 0.14474510920454986,
        ""id"": 161,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/162"",
        ""_type"": ""Gun"",
        ""dependability"": 0.80768738119289618,
        ""explosionFactor"": 0.25879317440967686,
        ""id"": 162,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/173"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Liquid Moose has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/173/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/172"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 172
    },
    ""id"": 173,
    ""name"": ""Liquid Moose"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/170"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/173""
        },
        ""id"": 170,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        },
        ""sku"": ""6946"",
        ""startsOn"": ""2012-10-17T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/171"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/173""
        },
        ""id"": 171,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""8113"",
        ""startsOn"": ""2012-11-21T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/166"",
        ""_type"": ""Gun"",
        ""dependability"": 0.81505577909529947,
        ""explosionFactor"": 0.75162557920051065,
        ""id"": 166,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/167"",
        ""_type"": ""Gun"",
        ""dependability"": 0.63903440844222648,
        ""explosionFactor"": 0.270295194476049,
        ""id"": 167,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/168"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90222209128654662,
        ""explosionFactor"": 0.95278293124995328,
        ""id"": 168,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/169"",
        ""dependability"": 0.045547633918722921,
        ""id"": 169,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/181"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unpleasant Budgerigar has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/181/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/180"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 180
    },
    ""id"": 181,
    ""name"": ""Unpleasant Budgerigar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/177"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/181""
        },
        ""id"": 177,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        },
        ""sku"": ""3567"",
        ""startsOn"": ""2012-11-20T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/178"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/181""
        },
        ""id"": 178,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        },
        ""sku"": ""1196"",
        ""startsOn"": ""2012-08-17T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/179"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/181""
        },
        ""id"": 179,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""3059"",
        ""startsOn"": ""2012-08-15T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/174"",
        ""dependability"": 0.394899630637327,
        ""id"": 174,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/175"",
        ""_type"": ""Gun"",
        ""dependability"": 0.081558652260135228,
        ""explosionFactor"": 0.79127121101658382,
        ""id"": 175,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/176"",
        ""dependability"": 0.056536442160856182,
        ""id"": 176,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/187"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Circular Donkey has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/187/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/186"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 186
    },
    ""id"": 187,
    ""name"": ""Circular Donkey"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/182"",
        ""dependability"": 0.98065767948546334,
        ""id"": 182,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/183"",
        ""dependability"": 0.70860286415955187,
        ""id"": 183,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/184"",
        ""dependability"": 0.47011568186344377,
        ""id"": 184,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/185"",
        ""dependability"": 0.75484091451151336,
        ""id"": 185,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/191"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Muffled Skunk has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/191/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/190"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 190
    },
    ""id"": 191,
    ""name"": ""Muffled Skunk"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/189"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/191""
        },
        ""id"": 189,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""9368"",
        ""startsOn"": ""2012-11-06T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/188"",
        ""dependability"": 0.46065895560228215,
        ""id"": 188,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/195"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Opulent Cony has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/195/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/194"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 194
    },
    ""id"": 195,
    ""name"": ""Opulent Cony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/192"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25580070459088344,
        ""explosionFactor"": 0.16042220460270634,
        ""id"": 192,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/193"",
        ""_type"": ""Gun"",
        ""dependability"": 0.81061353153112048,
        ""explosionFactor"": 0.08805828638750049,
        ""id"": 193,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/199"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""False Yak has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/199/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/198"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 198
    },
    ""id"": 199,
    ""name"": ""False Yak"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/196"",
        ""_type"": ""Gun"",
        ""dependability"": 0.99419132433561208,
        ""explosionFactor"": 0.14423586295183555,
        ""id"": 196,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/197"",
        ""dependability"": 0.79086931552312767,
        ""id"": 197,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/206"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Irritating Jerboa has cellulitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/206/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/205"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 205
    },
    ""id"": 206,
    ""name"": ""Irritating Jerboa"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/204"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/206""
        },
        ""id"": 204,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        },
        ""sku"": ""1830"",
        ""startsOn"": ""2012-10-15T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/200"",
        ""_type"": ""Gun"",
        ""dependability"": 0.7323837614303379,
        ""explosionFactor"": 0.50761523913015394,
        ""id"": 200,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/201"",
        ""dependability"": 0.016766073655693825,
        ""id"": 201,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/202"",
        ""_type"": ""Gun"",
        ""dependability"": 0.49904020992063,
        ""explosionFactor"": 0.024431658454440376,
        ""id"": 202,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/203"",
        ""dependability"": 0.73454835160381549,
        ""id"": 203,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/213"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Intentional Horse has exhaustion""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/213/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/212"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 212
    },
    ""id"": 213,
    ""name"": ""Intentional Horse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/209"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/213""
        },
        ""id"": 209,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        },
        ""sku"": ""8780"",
        ""startsOn"": ""2012-10-03T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/210"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/213""
        },
        ""id"": 210,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""8463"",
        ""startsOn"": ""2012-09-10T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/211"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/213""
        },
        ""id"": 211,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        },
        ""sku"": ""7213"",
        ""startsOn"": ""2012-08-31T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/207"",
        ""_type"": ""Gun"",
        ""dependability"": 0.15569770203702976,
        ""explosionFactor"": 0.37733321235390993,
        ""id"": 207,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/208"",
        ""dependability"": 0.833644103647044,
        ""id"": 208,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/220"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Wiggly Fox has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/220/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/219"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 219
    },
    ""id"": 220,
    ""instrument"": ""Valid Triangle"",
    ""name"": ""Wiggly Fox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/218"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/220"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 218,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        },
        ""sku"": ""4710"",
        ""startsOn"": ""2012-10-22T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/214"",
        ""dependability"": 0.17359142106659312,
        ""id"": 214,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/215"",
        ""dependability"": 0.25455521571196393,
        ""id"": 215,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/216"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25376790401235588,
        ""explosionFactor"": 0.7503185378156223,
        ""id"": 216,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/217"",
        ""_type"": ""Gun"",
        ""dependability"": 0.10895030158988679,
        ""explosionFactor"": 0.594569550172691,
        ""id"": 217,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/226"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Virtuous Lizard has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/226/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/225"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 225
    },
    ""id"": 226,
    ""name"": ""Virtuous Lizard"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/223"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/226""
        },
        ""id"": 223,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""sku"": ""3313"",
        ""startsOn"": ""2012-08-23T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/224"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/226""
        },
        ""id"": 224,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        },
        ""sku"": ""1247"",
        ""startsOn"": ""2012-10-04T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/221"",
        ""_type"": ""Gun"",
        ""dependability"": 0.948987161716906,
        ""explosionFactor"": 0.89625157504167952,
        ""id"": 221,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/222"",
        ""dependability"": 0.94767217335648468,
        ""id"": 222,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/231"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Juicy Wildcat has burn""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/231/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/230"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 230
    },
    ""id"": 231,
    ""name"": ""Juicy Wildcat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/228"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/231""
        },
        ""id"": 228,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        },
        ""sku"": ""5155"",
        ""startsOn"": ""2012-08-07T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/229"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/231""
        },
        ""id"": 229,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        },
        ""sku"": ""1398"",
        ""startsOn"": ""2012-10-14T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/227"",
        ""_type"": ""Gun"",
        ""dependability"": 0.7581098902775486,
        ""explosionFactor"": 0.38945486181855893,
        ""id"": 227,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/236"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Usable Zebra has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/236/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/235"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 235
    },
    ""id"": 236,
    ""name"": ""Usable Zebra"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/232"",
        ""dependability"": 0.35830364067028447,
        ""id"": 232,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/233"",
        ""dependability"": 0.3060957306558712,
        ""id"": 233,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/234"",
        ""_type"": ""Gun"",
        ""dependability"": 0.11951433872781431,
        ""explosionFactor"": 0.87801144778635887,
        ""id"": 234,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/242"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Arctic Dog has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/242/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/241"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 241
    },
    ""id"": 242,
    ""name"": ""Arctic Dog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/237"",
        ""dependability"": 0.4545277145013808,
        ""id"": 237,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/238"",
        ""dependability"": 0.953259715788653,
        ""id"": 238,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/239"",
        ""_type"": ""Gun"",
        ""dependability"": 0.0621657329900962,
        ""explosionFactor"": 0.42248607679385974,
        ""id"": 239,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/240"",
        ""_type"": ""Gun"",
        ""dependability"": 0.48224345291137855,
        ""explosionFactor"": 0.14662623784813389,
        ""id"": 240,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/248"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Accomplished Zebra has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/248/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/247"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 247
    },
    ""id"": 248,
    ""name"": ""Accomplished Zebra"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/244"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/248""
        },
        ""id"": 244,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""2699"",
        ""startsOn"": ""2012-10-23T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/245"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/248""
        },
        ""id"": 245,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        },
        ""sku"": ""2860"",
        ""startsOn"": ""2012-11-06T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/246"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/248""
        },
        ""id"": 246,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/22""
        },
        ""sku"": ""3749"",
        ""startsOn"": ""2012-10-31T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/243"",
        ""_type"": ""Gun"",
        ""dependability"": 0.40636681039182787,
        ""explosionFactor"": 0.2070306358891682,
        ""id"": 243,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/255"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Junior Elephant has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/255/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/254"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 254
    },
    ""id"": 255,
    ""name"": ""Junior Elephant"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/252"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/255""
        },
        ""id"": 252,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        },
        ""sku"": ""2403"",
        ""startsOn"": ""2012-10-16T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/253"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/255""
        },
        ""id"": 253,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        },
        ""sku"": ""7516"",
        ""startsOn"": ""2012-08-26T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/249"",
        ""dependability"": 0.16373482447291485,
        ""id"": 249,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/250"",
        ""_type"": ""Gun"",
        ""dependability"": 0.7296389666058305,
        ""explosionFactor"": 0.63909241586881338,
        ""id"": 250,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/251"",
        ""dependability"": 0.63672770123776412,
        ""id"": 251,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/260"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Gleeful Dog has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/260/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/259"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 259
    },
    ""id"": 260,
    ""name"": ""Gleeful Dog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/256"",
        ""dependability"": 0.77609781817351364,
        ""id"": 256,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/257"",
        ""_type"": ""Gun"",
        ""dependability"": 0.11068113060234167,
        ""explosionFactor"": 0.67581300515486531,
        ""id"": 257,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/258"",
        ""dependability"": 0.12069516168939656,
        ""id"": 258,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/264"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Noted Dormouse has tumour""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/264/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/263"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 263
    },
    ""id"": 264,
    ""name"": ""Noted Dormouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/262"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/264""
        },
        ""id"": 262,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""835"",
        ""startsOn"": ""2012-08-02T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/261"",
        ""_type"": ""Gun"",
        ""dependability"": 0.33225805048470297,
        ""explosionFactor"": 0.27884341277128244,
        ""id"": 261,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/268"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Royal Mustang has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/268/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/267"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 267
    },
    ""id"": 268,
    ""name"": ""Royal Mustang"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/266"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/268""
        },
        ""id"": 266,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""5713"",
        ""startsOn"": ""2012-10-07T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/265"",
        ""_type"": ""Gun"",
        ""dependability"": 0.48122841328439697,
        ""explosionFactor"": 0.15482852987704265,
        ""id"": 265,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/273"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Genuine Fox has adenoma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/273/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/272"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 272
    },
    ""id"": 273,
    ""instrument"": ""Leafy Mandolin"",
    ""name"": ""Genuine Fox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/271"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/273"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 271,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""69"",
        ""startsOn"": ""2012-08-30T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/269"",
        ""dependability"": 0.42519574539046534,
        ""id"": 269,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/270"",
        ""dependability"": 0.21005100999495527,
        ""id"": 270,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/282"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Mad Kitten has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/282/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/281"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 281
    },
    ""id"": 282,
    ""name"": ""Mad Kitten"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/278"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/282""
        },
        ""id"": 278,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        },
        ""sku"": ""9650"",
        ""startsOn"": ""2012-09-15T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/279"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/282""
        },
        ""id"": 279,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""9874"",
        ""startsOn"": ""2012-10-13T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/280"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/282""
        },
        ""id"": 280,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        },
        ""sku"": ""6116"",
        ""startsOn"": ""2012-08-24T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/274"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62113897345081859,
        ""explosionFactor"": 0.39161066682618606,
        ""id"": 274,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/275"",
        ""dependability"": 0.69199844342283834,
        ""id"": 275,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/276"",
        ""dependability"": 0.64636761306150237,
        ""id"": 276,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/277"",
        ""_type"": ""Gun"",
        ""dependability"": 0.17442011003122671,
        ""explosionFactor"": 0.99649168317974157,
        ""id"": 277,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/289"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Interesting Oryx has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/289/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/288"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 288
    },
    ""id"": 289,
    ""name"": ""Interesting Oryx"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/287"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/289""
        },
        ""id"": 287,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""7642"",
        ""startsOn"": ""2012-08-10T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/283"",
        ""_type"": ""Gun"",
        ""dependability"": 0.062739556218841835,
        ""explosionFactor"": 0.87462215166288526,
        ""id"": 283,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/284"",
        ""dependability"": 0.12554761819799787,
        ""id"": 284,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/285"",
        ""dependability"": 0.710002792864108,
        ""id"": 285,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/286"",
        ""_type"": ""Gun"",
        ""dependability"": 0.82964124848583776,
        ""explosionFactor"": 0.59788110507553494,
        ""id"": 286,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/293"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Giddy Cougar has thrush""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/293/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/292"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 292
    },
    ""id"": 293,
    ""name"": ""Giddy Cougar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/291"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/293""
        },
        ""id"": 291,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""3814"",
        ""startsOn"": ""2012-09-07T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/290"",
        ""dependability"": 0.30585768879664021,
        ""id"": 290,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/299"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Dreary Iguana has acne""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/299/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/298"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 298
    },
    ""id"": 299,
    ""name"": ""Dreary Iguana"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/294"",
        ""_type"": ""Gun"",
        ""dependability"": 0.57107814847076221,
        ""explosionFactor"": 0.30338337985024944,
        ""id"": 294,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/295"",
        ""dependability"": 0.254585371936944,
        ""id"": 295,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/296"",
        ""dependability"": 0.19572591371635251,
        ""id"": 296,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/297"",
        ""dependability"": 0.62534607836294276,
        ""id"": 297,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/304"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Noxious Eland has adenoma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/304/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/303"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 303
    },
    ""id"": 304,
    ""name"": ""Noxious Eland"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/302"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/304""
        },
        ""id"": 302,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/22""
        },
        ""sku"": ""5879"",
        ""startsOn"": ""2012-09-16T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/300"",
        ""dependability"": 0.33350031745317404,
        ""id"": 300,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/301"",
        ""_type"": ""Gun"",
        ""dependability"": 0.565179041384337,
        ""explosionFactor"": 0.50349506619549123,
        ""id"": 301,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/309"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Nippy Chameleon has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/309/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/308"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 308
    },
    ""id"": 309,
    ""name"": ""Nippy Chameleon"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/306"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/309""
        },
        ""id"": 306,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""sku"": ""3962"",
        ""startsOn"": ""2012-09-10T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/307"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/309""
        },
        ""id"": 307,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        },
        ""sku"": ""1213"",
        ""startsOn"": ""2012-09-22T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/305"",
        ""_type"": ""Gun"",
        ""dependability"": 0.81571374312774914,
        ""explosionFactor"": 0.65770597739969661,
        ""id"": 305,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/315"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Orange Musk has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/315/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/314"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 314
    },
    ""id"": 315,
    ""name"": ""Orange Musk"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/313"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/315""
        },
        ""id"": 313,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""4368"",
        ""startsOn"": ""2012-08-28T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/310"",
        ""dependability"": 0.73157461440729654,
        ""id"": 310,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/311"",
        ""dependability"": 0.0630130917127305,
        ""id"": 311,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/312"",
        ""dependability"": 0.61241268255394543,
        ""id"": 312,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/323"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Exciting Iguana has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/323/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/322"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 322
    },
    ""id"": 323,
    ""name"": ""Exciting Iguana"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/320"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/323""
        },
        ""id"": 320,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        },
        ""sku"": ""2062"",
        ""startsOn"": ""2012-11-04T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/321"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/323""
        },
        ""id"": 321,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        },
        ""sku"": ""3311"",
        ""startsOn"": ""2012-08-06T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/316"",
        ""_type"": ""Gun"",
        ""dependability"": 0.757949044815241,
        ""explosionFactor"": 0.011053988715193228,
        ""id"": 316,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/317"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2231480014618244,
        ""explosionFactor"": 0.95417733022672,
        ""id"": 317,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/318"",
        ""_type"": ""Gun"",
        ""dependability"": 0.083353918550235176,
        ""explosionFactor"": 0.75175227632362041,
        ""id"": 318,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/319"",
        ""dependability"": 0.51069451706050639,
        ""id"": 319,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/326"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Aromatic Bat has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/326/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/325"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 325
    },
    ""id"": 326,
    ""name"": ""Aromatic Bat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/324"",
        ""_type"": ""Gun"",
        ""dependability"": 0.45569478834778759,
        ""explosionFactor"": 0.65731728945733858,
        ""id"": 324,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/330"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Waterlogged Zebu has exhaustion""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/330/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/329"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 329
    },
    ""id"": 330,
    ""instrument"": ""Remorseful Violin"",
    ""name"": ""Waterlogged Zebu"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/328"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/330"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 328,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""6547"",
        ""startsOn"": ""2012-11-07T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/327"",
        ""dependability"": 0.93836543519905091,
        ""id"": 327,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/336"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Vengeful Cougar has epilepsy""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/336/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/335"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 335
    },
    ""id"": 336,
    ""name"": ""Vengeful Cougar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/331"",
        ""_type"": ""Gun"",
        ""dependability"": 0.17057236571357229,
        ""explosionFactor"": 0.60695132129218021,
        ""id"": 331,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/332"",
        ""_type"": ""Gun"",
        ""dependability"": 0.68043964015340419,
        ""explosionFactor"": 0.96576069386944208,
        ""id"": 332,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/333"",
        ""_type"": ""Gun"",
        ""dependability"": 0.26374962891626619,
        ""explosionFactor"": 0.84981674693981968,
        ""id"": 333,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/334"",
        ""_type"": ""Gun"",
        ""dependability"": 0.85885955479874254,
        ""explosionFactor"": 0.81427140152746413,
        ""id"": 334,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/341"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Sour Koodoo has cellulitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/341/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/340"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 340
    },
    ""id"": 341,
    ""name"": ""Sour Koodoo"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/338"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/341""
        },
        ""id"": 338,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""4226"",
        ""startsOn"": ""2012-08-18T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/339"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/341""
        },
        ""id"": 339,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        },
        ""sku"": ""9386"",
        ""startsOn"": ""2012-07-30T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/337"",
        ""dependability"": 0.39283058158719475,
        ""id"": 337,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/348"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Humiliating Koala has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/348/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/347"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 347
    },
    ""id"": 348,
    ""name"": ""Humiliating Koala"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/346"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/348""
        },
        ""id"": 346,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""5039"",
        ""startsOn"": ""2012-08-26T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/342"",
        ""_type"": ""Gun"",
        ""dependability"": 0.67240959763173458,
        ""explosionFactor"": 0.26541249838909714,
        ""id"": 342,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/343"",
        ""dependability"": 0.97158926770630727,
        ""id"": 343,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/344"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2944858750768406,
        ""explosionFactor"": 0.0622115321747081,
        ""id"": 344,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/345"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62502468359890617,
        ""explosionFactor"": 0.4256549093060451,
        ""id"": 345,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/355"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Worrisome Turtle has acne""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/355/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/354"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 354
    },
    ""id"": 355,
    ""instrument"": ""Kosher Conga"",
    ""name"": ""Worrisome Turtle"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/353"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/355"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 353,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""189"",
        ""startsOn"": ""2012-09-01T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/349"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2305126931660402,
        ""explosionFactor"": 0.2916809293868397,
        ""id"": 349,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/350"",
        ""dependability"": 0.056568757657226527,
        ""id"": 350,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/351"",
        ""dependability"": 0.39046856825727438,
        ""id"": 351,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/352"",
        ""dependability"": 0.62059084820588628,
        ""id"": 352,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/362"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Pleasant Eland has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/362/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/361"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 361
    },
    ""id"": 362,
    ""name"": ""Pleasant Eland"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/360"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/362""
        },
        ""id"": 360,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""6488"",
        ""startsOn"": ""2012-08-08T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/356"",
        ""dependability"": 0.79479655148219153,
        ""id"": 356,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/357"",
        ""_type"": ""Gun"",
        ""dependability"": 0.935930609207568,
        ""explosionFactor"": 0.29577908026789274,
        ""id"": 357,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/358"",
        ""_type"": ""Gun"",
        ""dependability"": 0.68788227796921608,
        ""explosionFactor"": 0.0076318429818525175,
        ""id"": 358,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/359"",
        ""dependability"": 0.60268388809761209,
        ""id"": 359,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/367"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Repulsive Zebu has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/367/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/366"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 366
    },
    ""id"": 367,
    ""name"": ""Repulsive Zebu"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/363"",
        ""dependability"": 0.88084413664454786,
        ""id"": 363,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/364"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90983884637702206,
        ""explosionFactor"": 0.58695791689071708,
        ""id"": 364,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/365"",
        ""dependability"": 0.64288392134145089,
        ""id"": 365,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/373"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Striped Musk-ox has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/373/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/372"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 372
    },
    ""id"": 373,
    ""instrument"": ""Parched Harpsichord"",
    ""name"": ""Striped Musk-ox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/368"",
        ""dependability"": 0.15479419853295862,
        ""id"": 368,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/369"",
        ""dependability"": 0.75952171895630738,
        ""id"": 369,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/370"",
        ""dependability"": 0.50741413445557193,
        ""id"": 370,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/371"",
        ""_type"": ""Gun"",
        ""dependability"": 0.8872460121695166,
        ""explosionFactor"": 0.13921777165458435,
        ""id"": 371,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/378"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Musty Ass has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/378/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/377"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 377
    },
    ""id"": 378,
    ""name"": ""Musty Ass"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/376"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/378""
        },
        ""id"": 376,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        },
        ""sku"": ""7468"",
        ""startsOn"": ""2012-11-04T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/374"",
        ""dependability"": 0.96025131594401381,
        ""id"": 374,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/375"",
        ""dependability"": 0.3765193072038327,
        ""id"": 375,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/382"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Flawed Addax has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/382/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/381"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 381
    },
    ""id"": 382,
    ""name"": ""Flawed Addax"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/380"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/382""
        },
        ""id"": 380,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""sku"": ""3005"",
        ""startsOn"": ""2012-11-09T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/379"",
        ""_type"": ""Gun"",
        ""dependability"": 0.41360468762628955,
        ""explosionFactor"": 0.7360251065045712,
        ""id"": 379,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/387"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Misty Stallion has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/387/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/386"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 386
    },
    ""id"": 387,
    ""name"": ""Misty Stallion"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/383"",
        ""_type"": ""Gun"",
        ""dependability"": 0.554588433147682,
        ""explosionFactor"": 0.94795409121921015,
        ""id"": 383,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/384"",
        ""dependability"": 0.466813616206317,
        ""id"": 384,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/385"",
        ""_type"": ""Gun"",
        ""dependability"": 0.049366200831423609,
        ""explosionFactor"": 0.17497924676862511,
        ""id"": 385,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/393"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Elliptical Dormouse has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/393/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/392"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 392
    },
    ""id"": 393,
    ""name"": ""Elliptical Dormouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/389"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/393""
        },
        ""id"": 389,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        },
        ""sku"": ""3691"",
        ""startsOn"": ""2012-10-11T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/390"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/393""
        },
        ""id"": 390,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        },
        ""sku"": ""8213"",
        ""startsOn"": ""2012-08-13T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/391"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/393""
        },
        ""id"": 391,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        },
        ""sku"": ""9602"",
        ""startsOn"": ""2012-10-01T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/388"",
        ""dependability"": 0.268462053159467,
        ""id"": 388,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/397"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Petty Squirrel has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/397/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/396"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 396
    },
    ""id"": 397,
    ""instrument"": ""International Oboe"",
    ""name"": ""Petty Squirrel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/394"",
        ""_type"": ""Gun"",
        ""dependability"": 0.75010416319132978,
        ""explosionFactor"": 0.15197034280373264,
        ""id"": 394,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/395"",
        ""dependability"": 0.059381714118356681,
        ""id"": 395,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/402"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tragic Bull has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/402/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/401"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 401
    },
    ""id"": 402,
    ""name"": ""Tragic Bull"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/399"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/402""
        },
        ""id"": 399,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        },
        ""sku"": ""86"",
        ""startsOn"": ""2012-11-13T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/400"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/402""
        },
        ""id"": 400,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""sku"": ""6938"",
        ""startsOn"": ""2012-09-12T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/398"",
        ""_type"": ""Gun"",
        ""dependability"": 0.65708311817472942,
        ""explosionFactor"": 0.47540447044903622,
        ""id"": 398,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/408"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Mortified Waterbuck has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/408/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/407"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 407
    },
    ""id"": 408,
    ""name"": ""Mortified Waterbuck"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/406"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/408""
        },
        ""id"": 406,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""2101"",
        ""startsOn"": ""2012-10-19T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/403"",
        ""_type"": ""Gun"",
        ""dependability"": 0.63437596644944327,
        ""explosionFactor"": 0.865567897849515,
        ""id"": 403,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/404"",
        ""dependability"": 0.18494691242694245,
        ""id"": 404,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/405"",
        ""_type"": ""Gun"",
        ""dependability"": 0.55232137886449761,
        ""explosionFactor"": 0.049373197392268663,
        ""id"": 405,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/417"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Stingy Elk has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/417/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/416"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 416
    },
    ""id"": 417,
    ""name"": ""Stingy Elk"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/413"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/417""
        },
        ""id"": 413,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        },
        ""sku"": ""2002"",
        ""startsOn"": ""2012-10-20T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/414"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/417""
        },
        ""id"": 414,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""325"",
        ""startsOn"": ""2012-09-28T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/415"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/417""
        },
        ""id"": 415,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        },
        ""sku"": ""595"",
        ""startsOn"": ""2012-08-25T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/409"",
        ""_type"": ""Gun"",
        ""dependability"": 0.28069916473734152,
        ""explosionFactor"": 0.32107535438662177,
        ""id"": 409,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/410"",
        ""_type"": ""Gun"",
        ""dependability"": 0.21698037871019002,
        ""explosionFactor"": 0.80077787619120344,
        ""id"": 410,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/411"",
        ""_type"": ""Gun"",
        ""dependability"": 0.39183967765040678,
        ""explosionFactor"": 0.99494740739229481,
        ""id"": 411,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/412"",
        ""dependability"": 0.10203418792320144,
        ""id"": 412,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/423"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Velvety Koala has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/423/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/422"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 422
    },
    ""id"": 423,
    ""name"": ""Velvety Koala"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/419"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/423""
        },
        ""id"": 419,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        },
        ""sku"": ""119"",
        ""startsOn"": ""2012-09-02T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/420"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/423""
        },
        ""id"": 420,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        },
        ""sku"": ""676"",
        ""startsOn"": ""2012-10-11T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/421"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/423""
        },
        ""id"": 421,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        },
        ""sku"": ""7764"",
        ""startsOn"": ""2012-08-05T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/418"",
        ""_type"": ""Gun"",
        ""dependability"": 0.178668521893522,
        ""explosionFactor"": 0.30701106102532288,
        ""id"": 418,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/426"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Front Ass has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/426/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/425"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 425
    },
    ""id"": 426,
    ""name"": ""Front Ass"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/424"",
        ""_type"": ""Gun"",
        ""dependability"": 0.087099782231775946,
        ""explosionFactor"": 0.088870908175069332,
        ""id"": 424,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/432"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Standard Dormouse has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/432/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/431"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 431
    },
    ""id"": 432,
    ""name"": ""Standard Dormouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/428"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/432""
        },
        ""id"": 428,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        },
        ""sku"": ""2123"",
        ""startsOn"": ""2012-08-25T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/429"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/432""
        },
        ""id"": 429,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        },
        ""sku"": ""6828"",
        ""startsOn"": ""2012-10-02T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/430"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/432""
        },
        ""id"": 430,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""8121"",
        ""startsOn"": ""2012-11-14T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/427"",
        ""dependability"": 0.292133392436492,
        ""id"": 427,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/440"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Ripe Mustang has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/440/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/439"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 439
    },
    ""id"": 440,
    ""name"": ""Ripe Mustang"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/437"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/440""
        },
        ""id"": 437,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""2418"",
        ""startsOn"": ""2012-11-05T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/438"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/440""
        },
        ""id"": 438,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""3462"",
        ""startsOn"": ""2012-10-03T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/433"",
        ""dependability"": 0.19525876976328846,
        ""id"": 433,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/434"",
        ""dependability"": 0.81667924943225423,
        ""id"": 434,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/435"",
        ""_type"": ""Gun"",
        ""dependability"": 0.73901487455657444,
        ""explosionFactor"": 0.054263034395064712,
        ""id"": 435,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/436"",
        ""dependability"": 0.685423597081296,
        ""id"": 436,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/448"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Upright Fox has acne""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/448/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/447"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 447
    },
    ""id"": 448,
    ""name"": ""Upright Fox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/444"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/448""
        },
        ""id"": 444,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""2405"",
        ""startsOn"": ""2012-09-22T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/445"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/448""
        },
        ""id"": 445,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        },
        ""sku"": ""1594"",
        ""startsOn"": ""2012-09-21T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/446"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/448""
        },
        ""id"": 446,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""6607"",
        ""startsOn"": ""2012-08-08T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/441"",
        ""_type"": ""Gun"",
        ""dependability"": 0.06821826895150275,
        ""explosionFactor"": 0.12711747415695221,
        ""id"": 441,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/442"",
        ""_type"": ""Gun"",
        ""dependability"": 0.32206874122939477,
        ""explosionFactor"": 0.081553471312650228,
        ""id"": 442,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/27""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/443"",
        ""dependability"": 0.32204226745387643,
        ""id"": 443,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/453"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""This Okapi has cellulitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/453/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/452"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 452
    },
    ""id"": 453,
    ""instrument"": ""Double Banjo"",
    ""name"": ""This Okapi"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/451"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/453"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 451,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""sku"": ""9459"",
        ""startsOn"": ""2012-09-06T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/449"",
        ""_type"": ""Gun"",
        ""dependability"": 0.41515089683940209,
        ""explosionFactor"": 0.98731163190086912,
        ""id"": 449,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/450"",
        ""dependability"": 0.28915231036448491,
        ""id"": 450,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/459"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Long Rhinoceros has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/459/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/458"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 458
    },
    ""id"": 459,
    ""name"": ""Long Rhinoceros"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/455"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/459""
        },
        ""id"": 455,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        },
        ""sku"": ""3843"",
        ""startsOn"": ""2012-09-12T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/456"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/459""
        },
        ""id"": 456,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        },
        ""sku"": ""5772"",
        ""startsOn"": ""2012-09-28T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/457"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/459""
        },
        ""id"": 457,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""1360"",
        ""startsOn"": ""2012-08-01T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/454"",
        ""_type"": ""Gun"",
        ""dependability"": 0.56069577744263033,
        ""explosionFactor"": 0.96733677432282672,
        ""id"": 454,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/32""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/464"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Able Lizard has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/464/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/463"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 463
    },
    ""id"": 464,
    ""name"": ""Able Lizard"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/461"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/464""
        },
        ""id"": 461,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        },
        ""sku"": ""83"",
        ""startsOn"": ""2012-10-14T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/462"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/464""
        },
        ""id"": 462,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""1261"",
        ""startsOn"": ""2012-10-04T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/460"",
        ""_type"": ""Gun"",
        ""dependability"": 0.33473063555300731,
        ""explosionFactor"": 0.91902015959798367,
        ""id"": 460,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/471"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Nasty Civet has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/471/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/470"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 470
    },
    ""id"": 471,
    ""instrument"": ""Passionate Saxophone"",
    ""name"": ""Nasty Civet"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/467"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/471"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 467,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""9937"",
        ""startsOn"": ""2012-09-24T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/468"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/471"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 468,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        },
        ""sku"": ""6123"",
        ""startsOn"": ""2012-11-07T08:17:25.8226085Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/469"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/471"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 469,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        },
        ""sku"": ""8805"",
        ""startsOn"": ""2012-11-15T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/465"",
        ""_type"": ""Gun"",
        ""dependability"": 0.20560774775483076,
        ""explosionFactor"": 0.82368512117475512,
        ""id"": 465,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/466"",
        ""_type"": ""Gun"",
        ""dependability"": 0.37981547526075293,
        ""explosionFactor"": 0.21080097007136836,
        ""id"": 466,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/477"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Aggressive Horse has shingles""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/477/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/476"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 476
    },
    ""id"": 477,
    ""name"": ""Aggressive Horse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/472"",
        ""_type"": ""Gun"",
        ""dependability"": 0.26620785857839874,
        ""explosionFactor"": 0.13622280309732202,
        ""id"": 472,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/473"",
        ""dependability"": 0.75207807624343692,
        ""id"": 473,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/474"",
        ""_type"": ""Gun"",
        ""dependability"": 0.13844137924650748,
        ""explosionFactor"": 0.24904890835706559,
        ""id"": 474,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/475"",
        ""_type"": ""Gun"",
        ""dependability"": 0.80028508314876123,
        ""explosionFactor"": 0.47464064856741606,
        ""id"": 475,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/483"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""General Lizard has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/483/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/482"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 482
    },
    ""id"": 483,
    ""name"": ""General Lizard"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/481"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/483""
        },
        ""id"": 481,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""6214"",
        ""startsOn"": ""2012-11-23T08:17:25.8226085Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/478"",
        ""dependability"": 0.31918975679166139,
        ""id"": 478,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/479"",
        ""dependability"": 0.12062292411952416,
        ""id"": 479,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/480"",
        ""_type"": ""Gun"",
        ""dependability"": 0.41875019083672677,
        ""explosionFactor"": 0.8646915181841196,
        ""id"": 480,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/488"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Common Porcupine has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/488/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/487"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 487
    },
    ""id"": 488,
    ""name"": ""Common Porcupine"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/486"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/488""
        },
        ""id"": 486,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        },
        ""sku"": ""692"",
        ""startsOn"": ""2012-11-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/484"",
        ""_type"": ""Gun"",
        ""dependability"": 0.499898404581425,
        ""explosionFactor"": 0.35769353544232135,
        ""id"": 484,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/485"",
        ""_type"": ""Gun"",
        ""dependability"": 0.38303974894016968,
        ""explosionFactor"": 0.68936736913834107,
        ""id"": 485,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/493"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Delirious Steer has epilepsy""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/493/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/492"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 492
    },
    ""id"": 493,
    ""name"": ""Delirious Steer"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/491"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/493""
        },
        ""id"": 491,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        },
        ""sku"": ""715"",
        ""startsOn"": ""2012-11-07T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/489"",
        ""_type"": ""Gun"",
        ""dependability"": 0.78848922615334827,
        ""explosionFactor"": 0.80024943770852375,
        ""id"": 489,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/490"",
        ""dependability"": 0.075227563770128208,
        ""id"": 490,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/499"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unwilling Vicuna has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/499/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/498"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 498
    },
    ""id"": 499,
    ""instrument"": ""Sunny Bass"",
    ""name"": ""Unwilling Vicuna"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/495"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/499"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 495,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""4312"",
        ""startsOn"": ""2012-09-14T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/496"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/499"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 496,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""3494"",
        ""startsOn"": ""2012-11-18T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/497"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/499"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 497,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""5546"",
        ""startsOn"": ""2012-09-17T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/494"",
        ""_type"": ""Gun"",
        ""dependability"": 0.35962143091467275,
        ""explosionFactor"": 0.76123659627569684,
        ""id"": 494,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/508"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Striking Cony has tumour""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/508/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/507"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 507
    },
    ""id"": 508,
    ""name"": ""Striking Cony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/504"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/508""
        },
        ""id"": 504,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/62""
        },
        ""sku"": ""3715"",
        ""startsOn"": ""2012-08-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/505"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/508""
        },
        ""id"": 505,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""2828"",
        ""startsOn"": ""2012-08-21T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/506"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/508""
        },
        ""id"": 506,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        },
        ""sku"": ""5204"",
        ""startsOn"": ""2012-08-28T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/500"",
        ""_type"": ""Gun"",
        ""dependability"": 0.69148522088838982,
        ""explosionFactor"": 0.42467084081129675,
        ""id"": 500,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/501"",
        ""dependability"": 0.012579219421641537,
        ""id"": 501,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/502"",
        ""dependability"": 0.58408057763431254,
        ""id"": 502,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/503"",
        ""_type"": ""Gun"",
        ""dependability"": 0.99835823429671966,
        ""explosionFactor"": 0.64983946068670573,
        ""id"": 503,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/514"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Hospitable Fish has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/514/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/513"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 513
    },
    ""id"": 514,
    ""name"": ""Hospitable Fish"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/511"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/514""
        },
        ""id"": 511,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        },
        ""sku"": ""2756"",
        ""startsOn"": ""2012-08-17T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/512"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/514""
        },
        ""id"": 512,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        },
        ""sku"": ""568"",
        ""startsOn"": ""2012-09-30T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/509"",
        ""dependability"": 0.51349838288198146,
        ""id"": 509,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/510"",
        ""_type"": ""Gun"",
        ""dependability"": 0.55038687286450849,
        ""explosionFactor"": 0.87601948756539239,
        ""id"": 510,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/520"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Villainous Wildcat has adenoma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/520/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/519"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 519
    },
    ""id"": 520,
    ""instrument"": ""United Conga"",
    ""name"": ""Villainous Wildcat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/518"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/520"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 518,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""5416"",
        ""startsOn"": ""2012-11-19T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/515"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62374374672013511,
        ""explosionFactor"": 0.683380618544007,
        ""id"": 515,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/516"",
        ""_type"": ""Gun"",
        ""dependability"": 0.63174015313002285,
        ""explosionFactor"": 0.4168688247058861,
        ""id"": 516,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/517"",
        ""dependability"": 0.18346858405669619,
        ""id"": 517,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/527"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Vigorous Chinchilla has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/527/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/526"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 526
    },
    ""id"": 527,
    ""name"": ""Vigorous Chinchilla"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/525"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/527""
        },
        ""id"": 525,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        },
        ""sku"": ""8896"",
        ""startsOn"": ""2012-08-29T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/521"",
        ""dependability"": 0.8183659970845869,
        ""id"": 521,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/522"",
        ""_type"": ""Gun"",
        ""dependability"": 0.15338687279885024,
        ""explosionFactor"": 0.82441350064445917,
        ""id"": 522,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/523"",
        ""_type"": ""Gun"",
        ""dependability"": 0.51448316616680623,
        ""explosionFactor"": 0.35783095627922146,
        ""id"": 523,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/524"",
        ""dependability"": 0.8780741178794178,
        ""id"": 524,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/32""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/533"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Envious Hartebeest has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/533/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/532"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 532
    },
    ""id"": 533,
    ""name"": ""Envious Hartebeest"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/529"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/533""
        },
        ""id"": 529,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""sku"": ""8674"",
        ""startsOn"": ""2012-11-09T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/530"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/533""
        },
        ""id"": 530,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""sku"": ""280"",
        ""startsOn"": ""2012-11-03T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/531"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/533""
        },
        ""id"": 531,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""2629"",
        ""startsOn"": ""2012-09-19T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/528"",
        ""dependability"": 0.37624636589374688,
        ""id"": 528,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/538"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Faraway Mouse has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/538/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/537"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 537
    },
    ""id"": 538,
    ""name"": ""Faraway Mouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/536"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/538""
        },
        ""id"": 536,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""1194"",
        ""startsOn"": ""2012-11-11T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/534"",
        ""_type"": ""Gun"",
        ""dependability"": 0.18445940510577494,
        ""explosionFactor"": 0.40950142657826721,
        ""id"": 534,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/535"",
        ""dependability"": 0.21805933081454565,
        ""id"": 535,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/545"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Silly Ocelot has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/545/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/544"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 544
    },
    ""id"": 545,
    ""name"": ""Silly Ocelot"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/542"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/545""
        },
        ""id"": 542,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        },
        ""sku"": ""2312"",
        ""startsOn"": ""2012-08-28T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/543"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/545""
        },
        ""id"": 543,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""sku"": ""9583"",
        ""startsOn"": ""2012-08-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/539"",
        ""_type"": ""Gun"",
        ""dependability"": 0.03458013061181648,
        ""explosionFactor"": 0.19269449831577692,
        ""id"": 539,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/540"",
        ""dependability"": 0.49947321019110885,
        ""id"": 540,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/541"",
        ""_type"": ""Gun"",
        ""dependability"": 0.633205270689542,
        ""explosionFactor"": 0.15255942202758016,
        ""id"": 541,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/551"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tinted Chameleon has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/551/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/550"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 550
    },
    ""id"": 551,
    ""name"": ""Tinted Chameleon"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/548"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/551""
        },
        ""id"": 548,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""sku"": ""9296"",
        ""startsOn"": ""2012-08-15T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/549"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/551""
        },
        ""id"": 549,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        },
        ""sku"": ""9765"",
        ""startsOn"": ""2012-10-08T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/546"",
        ""dependability"": 0.7199365504644516,
        ""id"": 546,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/547"",
        ""dependability"": 0.14558214281945589,
        ""id"": 547,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/557"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Last Elk has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/557/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/556"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 556
    },
    ""id"": 557,
    ""name"": ""Last Elk"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/555"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/557""
        },
        ""id"": 555,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        },
        ""sku"": ""7230"",
        ""startsOn"": ""2012-08-21T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/552"",
        ""_type"": ""Gun"",
        ""dependability"": 0.085082680492234733,
        ""explosionFactor"": 0.50744278380062557,
        ""id"": 552,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/553"",
        ""_type"": ""Gun"",
        ""dependability"": 0.212014999339364,
        ""explosionFactor"": 0.8488212920952688,
        ""id"": 553,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/554"",
        ""dependability"": 0.53632120487108881,
        ""id"": 554,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/563"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Jam-packed Fish has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/563/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/562"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 562
    },
    ""id"": 563,
    ""name"": ""Jam-packed Fish"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/560"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/563""
        },
        ""id"": 560,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""6731"",
        ""startsOn"": ""2012-10-14T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/561"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/563""
        },
        ""id"": 561,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""6184"",
        ""startsOn"": ""2012-08-14T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/558"",
        ""_type"": ""Gun"",
        ""dependability"": 0.7959571544993469,
        ""explosionFactor"": 0.47098091126930941,
        ""id"": 558,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/559"",
        ""_type"": ""Gun"",
        ""dependability"": 0.92584178500149483,
        ""explosionFactor"": 0.25466041371908987,
        ""id"": 559,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/569"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Dismal Colt has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/569/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/568"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 568
    },
    ""id"": 569,
    ""name"": ""Dismal Colt"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/565"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/569""
        },
        ""id"": 565,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        },
        ""sku"": ""4806"",
        ""startsOn"": ""2012-08-22T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/566"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/569""
        },
        ""id"": 566,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""sku"": ""7653"",
        ""startsOn"": ""2012-11-10T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/567"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/569""
        },
        ""id"": 567,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""8350"",
        ""startsOn"": ""2012-09-09T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/564"",
        ""dependability"": 0.26223863254405494,
        ""id"": 564,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/576"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Harmonious Rabbit has imperteigo""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/576/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/575"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 575
    },
    ""id"": 576,
    ""name"": ""Harmonious Rabbit"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/573"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/576""
        },
        ""id"": 573,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""sku"": ""3103"",
        ""startsOn"": ""2012-11-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/574"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/576""
        },
        ""id"": 574,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""8211"",
        ""startsOn"": ""2012-08-04T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/570"",
        ""_type"": ""Gun"",
        ""dependability"": 0.55643073774708007,
        ""explosionFactor"": 0.95735458562027409,
        ""id"": 570,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/571"",
        ""_type"": ""Gun"",
        ""dependability"": 0.86265552270349843,
        ""explosionFactor"": 0.87760720023773942,
        ""id"": 571,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/572"",
        ""_type"": ""Gun"",
        ""dependability"": 0.57067469999691223,
        ""explosionFactor"": 0.19209927096595023,
        ""id"": 572,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/583"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Shameless Seal has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/583/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/582"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 582
    },
    ""id"": 583,
    ""name"": ""Shameless Seal"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/581"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/583""
        },
        ""id"": 581,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        },
        ""sku"": ""1497"",
        ""startsOn"": ""2012-08-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/577"",
        ""dependability"": 0.85119526034742377,
        ""id"": 577,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/578"",
        ""_type"": ""Gun"",
        ""dependability"": 0.4733103790661834,
        ""explosionFactor"": 0.81951267915755166,
        ""id"": 578,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/579"",
        ""dependability"": 0.58021844438287362,
        ""id"": 579,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/580"",
        ""dependability"": 0.68123328996879673,
        ""id"": 580,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/591"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unruly Crocodile has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/591/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/590"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 590
    },
    ""id"": 591,
    ""name"": ""Unruly Crocodile"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/587"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/591""
        },
        ""id"": 587,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        },
        ""sku"": ""4680"",
        ""startsOn"": ""2012-09-14T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/588"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/591""
        },
        ""id"": 588,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        },
        ""sku"": ""246"",
        ""startsOn"": ""2012-10-06T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/589"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/591""
        },
        ""id"": 589,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""5048"",
        ""startsOn"": ""2012-10-30T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/584"",
        ""dependability"": 0.52477693395911573,
        ""id"": 584,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/585"",
        ""dependability"": 0.97772621082967437,
        ""id"": 585,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/586"",
        ""_type"": ""Gun"",
        ""dependability"": 0.76902401250275965,
        ""explosionFactor"": 0.54257273978673515,
        ""id"": 586,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/594"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Delirious Weasel has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/594/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/593"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 593
    },
    ""id"": 594,
    ""name"": ""Delirious Weasel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/592"",
        ""_type"": ""Gun"",
        ""dependability"": 0.91920106155760639,
        ""explosionFactor"": 0.662804403650949,
        ""id"": 592,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/599"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Different Fox has dehydration""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/599/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/598"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 598
    },
    ""id"": 599,
    ""name"": ""Different Fox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/597"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/599""
        },
        ""id"": 597,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""sku"": ""8989"",
        ""startsOn"": ""2012-10-29T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/595"",
        ""_type"": ""Gun"",
        ""dependability"": 0.77647672070957563,
        ""explosionFactor"": 0.9457337008536485,
        ""id"": 595,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/596"",
        ""_type"": ""Gun"",
        ""dependability"": 0.24951164855133354,
        ""explosionFactor"": 0.81119443188011386,
        ""id"": 596,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/604"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Careless Springbok has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/604/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/603"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 603
    },
    ""id"": 604,
    ""name"": ""Careless Springbok"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/602"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/604""
        },
        ""id"": 602,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        },
        ""sku"": ""8806"",
        ""startsOn"": ""2012-09-11T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/600"",
        ""dependability"": 0.11650221148342929,
        ""id"": 600,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/27""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/601"",
        ""dependability"": 0.19960385849680931,
        ""id"": 601,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/27""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/612"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Optimal Moose has epilepsy""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/612/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/611"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 611
    },
    ""id"": 612,
    ""instrument"": ""Quiet Banjo"",
    ""name"": ""Optimal Moose"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/609"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/612"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 609,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""882"",
        ""startsOn"": ""2012-11-15T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/610"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/612"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 610,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""7993"",
        ""startsOn"": ""2012-07-29T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/605"",
        ""dependability"": 0.81551282471768227,
        ""id"": 605,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/606"",
        ""dependability"": 0.32640546389222397,
        ""id"": 606,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/607"",
        ""_type"": ""Gun"",
        ""dependability"": 0.80383030455737858,
        ""explosionFactor"": 0.2695971635494368,
        ""id"": 607,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/608"",
        ""_type"": ""Gun"",
        ""dependability"": 0.78924140836542533,
        ""explosionFactor"": 0.030787626761378546,
        ""id"": 608,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/621"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Jam-packed Buffalo has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/621/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/620"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 620
    },
    ""id"": 621,
    ""name"": ""Jam-packed Buffalo"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/617"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/621""
        },
        ""id"": 617,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""426"",
        ""startsOn"": ""2012-09-18T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/618"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/621""
        },
        ""id"": 618,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""sku"": ""3575"",
        ""startsOn"": ""2012-08-28T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/619"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/621""
        },
        ""id"": 619,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        },
        ""sku"": ""9324"",
        ""startsOn"": ""2012-10-15T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/613"",
        ""_type"": ""Gun"",
        ""dependability"": 0.13895222178611541,
        ""explosionFactor"": 0.36069555224883165,
        ""id"": 613,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/614"",
        ""_type"": ""Gun"",
        ""dependability"": 0.23449112532403837,
        ""explosionFactor"": 0.31344258753277482,
        ""id"": 614,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/615"",
        ""_type"": ""Gun"",
        ""dependability"": 0.34079358416646421,
        ""explosionFactor"": 0.59213767181715815,
        ""id"": 615,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/616"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90918260715398125,
        ""explosionFactor"": 0.25666905392737549,
        ""id"": 616,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/627"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Wild Pony has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/627/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/626"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 626
    },
    ""id"": 627,
    ""name"": ""Wild Pony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/625"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/627""
        },
        ""id"": 625,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""sku"": ""5996"",
        ""startsOn"": ""2012-09-24T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/622"",
        ""_type"": ""Gun"",
        ""dependability"": 0.70209134402782258,
        ""explosionFactor"": 0.24648718733642586,
        ""id"": 622,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/623"",
        ""_type"": ""Gun"",
        ""dependability"": 0.44313475230854693,
        ""explosionFactor"": 0.81811852511862226,
        ""id"": 623,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/624"",
        ""dependability"": 0.47579882083265057,
        ""id"": 624,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/635"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unhealthy Waterbuck has exhaustion""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/635/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/634"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 634
    },
    ""id"": 635,
    ""name"": ""Unhealthy Waterbuck"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/631"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/635""
        },
        ""id"": 631,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        },
        ""sku"": ""9752"",
        ""startsOn"": ""2012-08-08T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/632"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/635""
        },
        ""id"": 632,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""sku"": ""2133"",
        ""startsOn"": ""2012-08-11T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/633"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/635""
        },
        ""id"": 633,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        },
        ""sku"": ""3777"",
        ""startsOn"": ""2012-08-09T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/628"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90300568654341884,
        ""explosionFactor"": 0.86948724597202953,
        ""id"": 628,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/12""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/629"",
        ""_type"": ""Gun"",
        ""dependability"": 0.4071854052167318,
        ""explosionFactor"": 0.42813205878628979,
        ""id"": 629,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/630"",
        ""_type"": ""Gun"",
        ""dependability"": 0.48165043465869989,
        ""explosionFactor"": 0.34303283055454159,
        ""id"": 630,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/641"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Periodic Lion has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/641/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/640"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 640
    },
    ""id"": 641,
    ""name"": ""Periodic Lion"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/637"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/641""
        },
        ""id"": 637,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""2083"",
        ""startsOn"": ""2012-09-14T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/638"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/641""
        },
        ""id"": 638,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        },
        ""sku"": ""5869"",
        ""startsOn"": ""2012-11-03T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/639"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/641""
        },
        ""id"": 639,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        },
        ""sku"": ""6249"",
        ""startsOn"": ""2012-10-06T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/636"",
        ""_type"": ""Gun"",
        ""dependability"": 0.47305370330487084,
        ""explosionFactor"": 0.23491171478988218,
        ""id"": 636,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/644"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Testy Ape has thrush""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/644/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/643"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 643
    },
    ""id"": 644,
    ""name"": ""Testy Ape"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/642"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62434313754753357,
        ""explosionFactor"": 0.39408275875918697,
        ""id"": 642,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/647"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Mammoth Porpoise has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/647/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/646"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 646
    },
    ""id"": 647,
    ""name"": ""Mammoth Porpoise"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/645"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25375715980946884,
        ""explosionFactor"": 0.72726977603848553,
        ""id"": 645,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/653"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Formal Zebra has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/653/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/652"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 652
    },
    ""id"": 653,
    ""name"": ""Formal Zebra"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/651"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/653""
        },
        ""id"": 651,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""sku"": ""2230"",
        ""startsOn"": ""2012-08-08T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/648"",
        ""dependability"": 0.38767513650826885,
        ""id"": 648,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/649"",
        ""dependability"": 0.19657972138215774,
        ""id"": 649,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/650"",
        ""dependability"": 0.65816840886053085,
        ""id"": 650,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/658"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Smart Kid has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/658/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/657"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 657
    },
    ""id"": 658,
    ""name"": ""Smart Kid"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/655"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/658""
        },
        ""id"": 655,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        },
        ""sku"": ""3421"",
        ""startsOn"": ""2012-10-09T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/656"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/658""
        },
        ""id"": 656,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        },
        ""sku"": ""5787"",
        ""startsOn"": ""2012-10-30T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/654"",
        ""dependability"": 0.10084458538370421,
        ""id"": 654,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/665"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Agreeable Fish has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/665/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/664"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 664
    },
    ""id"": 665,
    ""name"": ""Agreeable Fish"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/663"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/665""
        },
        ""id"": 663,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        },
        ""sku"": ""2304"",
        ""startsOn"": ""2012-08-28T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/659"",
        ""dependability"": 0.84222522789716037,
        ""id"": 659,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/660"",
        ""dependability"": 0.87904214667111735,
        ""id"": 660,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/661"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25835776573901892,
        ""explosionFactor"": 0.85519865101911063,
        ""id"": 661,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/662"",
        ""dependability"": 0.11534068878523106,
        ""id"": 662,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/671"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Adventurous Weasel has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/671/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/670"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 670
    },
    ""id"": 671,
    ""name"": ""Adventurous Weasel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/669"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/671""
        },
        ""id"": 669,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        },
        ""sku"": ""4899"",
        ""startsOn"": ""2012-10-13T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/666"",
        ""dependability"": 0.045462986475537988,
        ""id"": 666,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/667"",
        ""_type"": ""Gun"",
        ""dependability"": 0.61773278313583357,
        ""explosionFactor"": 0.89223495726112045,
        ""id"": 667,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/27""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/668"",
        ""_type"": ""Gun"",
        ""dependability"": 0.33947452592639,
        ""explosionFactor"": 0.3382196115973497,
        ""id"": 668,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/680"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Joyous Frog has burn""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/680/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/679"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 679
    },
    ""id"": 680,
    ""instrument"": ""Unsightly Cello"",
    ""name"": ""Joyous Frog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/676"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/680"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 676,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""5843"",
        ""startsOn"": ""2012-09-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/677"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/680"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 677,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""sku"": ""3420"",
        ""startsOn"": ""2012-09-27T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/678"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/680"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 678,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        },
        ""sku"": ""6611"",
        ""startsOn"": ""2012-11-05T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/672"",
        ""_type"": ""Gun"",
        ""dependability"": 0.504311840284761,
        ""explosionFactor"": 0.9599488116614282,
        ""id"": 672,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/673"",
        ""dependability"": 0.31614923538460826,
        ""id"": 673,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/56""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/674"",
        ""_type"": ""Gun"",
        ""dependability"": 0.73095705580476533,
        ""explosionFactor"": 0.56094341239004553,
        ""id"": 674,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/675"",
        ""dependability"": 0.1767708306092633,
        ""id"": 675,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/687"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tragic Impala has exhaustion""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/687/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/686"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 686
    },
    ""id"": 687,
    ""instrument"": ""Feisty Metallophone"",
    ""name"": ""Tragic Impala"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/684"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/687"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 684,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        },
        ""sku"": ""2656"",
        ""startsOn"": ""2012-10-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/685"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/687"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 685,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        },
        ""sku"": ""20"",
        ""startsOn"": ""2012-11-17T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/681"",
        ""dependability"": 0.61531559639392219,
        ""id"": 681,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/682"",
        ""dependability"": 0.8932867450142683,
        ""id"": 682,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/683"",
        ""dependability"": 0.32580277338894215,
        ""id"": 683,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/693"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Remorseful Snake has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/693/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/692"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 692
    },
    ""id"": 693,
    ""name"": ""Remorseful Snake"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/688"",
        ""dependability"": 0.31706349333611478,
        ""id"": 688,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/689"",
        ""dependability"": 0.65923603328840619,
        ""id"": 689,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/690"",
        ""dependability"": 0.99049700283934228,
        ""id"": 690,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/691"",
        ""dependability"": 0.287682710349412,
        ""id"": 691,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/698"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Vibrant Mouse has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/698/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/697"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 697
    },
    ""id"": 698,
    ""name"": ""Vibrant Mouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/695"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/698""
        },
        ""id"": 695,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        },
        ""sku"": ""2498"",
        ""startsOn"": ""2012-08-02T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/696"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/698""
        },
        ""id"": 696,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""sku"": ""9633"",
        ""startsOn"": ""2012-09-05T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/694"",
        ""_type"": ""Gun"",
        ""dependability"": 0.54670897058523682,
        ""explosionFactor"": 0.82659449466811241,
        ""id"": 694,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/22""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/706"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Dearest Mink has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/706/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/705"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 705
    },
    ""id"": 706,
    ""name"": ""Dearest Mink"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/702"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/706""
        },
        ""id"": 702,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        },
        ""sku"": ""837"",
        ""startsOn"": ""2012-09-11T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/703"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/706""
        },
        ""id"": 703,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""sku"": ""8308"",
        ""startsOn"": ""2012-10-15T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/704"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/706""
        },
        ""id"": 704,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        },
        ""sku"": ""3291"",
        ""startsOn"": ""2012-09-16T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/699"",
        ""_type"": ""Gun"",
        ""dependability"": 0.29825210305780736,
        ""explosionFactor"": 0.909924442837911,
        ""id"": 699,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/700"",
        ""_type"": ""Gun"",
        ""dependability"": 0.34017744117424703,
        ""explosionFactor"": 0.28240383057035684,
        ""id"": 700,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/701"",
        ""dependability"": 0.097922481642068585,
        ""id"": 701,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/713"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Offensive Porcupine has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/713/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/712"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 712
    },
    ""id"": 713,
    ""name"": ""Offensive Porcupine"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/710"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/713""
        },
        ""id"": 710,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        },
        ""sku"": ""1940"",
        ""startsOn"": ""2012-08-05T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/711"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/713""
        },
        ""id"": 711,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""2642"",
        ""startsOn"": ""2012-09-23T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/707"",
        ""_type"": ""Gun"",
        ""dependability"": 0.193527275786515,
        ""explosionFactor"": 0.89234567475148741,
        ""id"": 707,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/708"",
        ""dependability"": 0.080388011448265989,
        ""id"": 708,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/709"",
        ""dependability"": 0.4419913969198202,
        ""id"": 709,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/719"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Austere Kitten has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/719/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/718"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 718
    },
    ""id"": 719,
    ""instrument"": ""Poor Cornet"",
    ""name"": ""Austere Kitten"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/716"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/719"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 716,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        },
        ""sku"": ""3936"",
        ""startsOn"": ""2012-10-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/717"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/719"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 717,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        },
        ""sku"": ""888"",
        ""startsOn"": ""2012-09-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/714"",
        ""dependability"": 0.8006025575104182,
        ""id"": 714,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/715"",
        ""_type"": ""Gun"",
        ""dependability"": 0.15380430089021302,
        ""explosionFactor"": 0.41694797408624923,
        ""id"": 715,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/723"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unrealistic Mole has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/723/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/722"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 722
    },
    ""id"": 723,
    ""name"": ""Unrealistic Mole"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/721"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/723""
        },
        ""id"": 721,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""1608"",
        ""startsOn"": ""2012-11-22T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/720"",
        ""_type"": ""Gun"",
        ""dependability"": 0.093994443348606321,
        ""explosionFactor"": 0.47582452533572189,
        ""id"": 720,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/727"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Bubbly Weasel has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/727/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/726"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 726
    },
    ""id"": 727,
    ""name"": ""Bubbly Weasel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/724"",
        ""dependability"": 0.62512127339147094,
        ""id"": 724,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/725"",
        ""dependability"": 0.20223273439436812,
        ""id"": 725,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/733"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Starry Cony has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/733/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/732"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 732
    },
    ""id"": 733,
    ""instrument"": ""Alarmed Ukulele"",
    ""name"": ""Starry Cony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/731"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/733"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 731,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""sku"": ""1675"",
        ""startsOn"": ""2012-10-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/728"",
        ""_type"": ""Gun"",
        ""dependability"": 0.50449639721982942,
        ""explosionFactor"": 0.048267745901023854,
        ""id"": 728,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/12""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/729"",
        ""_type"": ""Gun"",
        ""dependability"": 0.96245147844890666,
        ""explosionFactor"": 0.63595969445815292,
        ""id"": 729,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/730"",
        ""_type"": ""Gun"",
        ""dependability"": 0.436761171760392,
        ""explosionFactor"": 0.50780882290928109,
        ""id"": 730,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/742"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Familiar Hog has acne""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/742/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/741"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 741
    },
    ""id"": 742,
    ""name"": ""Familiar Hog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/738"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/742""
        },
        ""id"": 738,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        },
        ""sku"": ""9377"",
        ""startsOn"": ""2012-08-27T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/739"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/742""
        },
        ""id"": 739,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""7236"",
        ""startsOn"": ""2012-11-02T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/740"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/742""
        },
        ""id"": 740,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        },
        ""sku"": ""8163"",
        ""startsOn"": ""2012-11-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/734"",
        ""dependability"": 0.1325137788115599,
        ""id"": 734,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/735"",
        ""dependability"": 0.12312756251689398,
        ""id"": 735,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/736"",
        ""dependability"": 0.47489574666828649,
        ""id"": 736,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/737"",
        ""dependability"": 0.92120676949676439,
        ""id"": 737,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/62""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/749"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Disloyal Tiger has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/749/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/748"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 748
    },
    ""id"": 749,
    ""name"": ""Disloyal Tiger"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/747"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/749""
        },
        ""id"": 747,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""7612"",
        ""startsOn"": ""2012-10-05T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/743"",
        ""_type"": ""Gun"",
        ""dependability"": 0.14932444745177612,
        ""explosionFactor"": 0.75534214999309846,
        ""id"": 743,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/744"",
        ""_type"": ""Gun"",
        ""dependability"": 0.74377880280082054,
        ""explosionFactor"": 0.96569481956106373,
        ""id"": 744,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/745"",
        ""_type"": ""Gun"",
        ""dependability"": 0.17161818368901413,
        ""explosionFactor"": 0.67906631700651088,
        ""id"": 745,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/746"",
        ""dependability"": 0.71827134057798947,
        ""id"": 746,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/12""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/755"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Elastic Camel has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/755/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/754"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 754
    },
    ""id"": 755,
    ""name"": ""Elastic Camel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/750"",
        ""_type"": ""Gun"",
        ""dependability"": 0.73001809033100407,
        ""explosionFactor"": 0.040239774640761214,
        ""id"": 750,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/751"",
        ""_type"": ""Gun"",
        ""dependability"": 0.21597394310681797,
        ""explosionFactor"": 0.67412320416147042,
        ""id"": 751,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/752"",
        ""_type"": ""Gun"",
        ""dependability"": 0.57624248861160243,
        ""explosionFactor"": 0.54154054566358245,
        ""id"": 752,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/753"",
        ""_type"": ""Gun"",
        ""dependability"": 0.14400822443142916,
        ""explosionFactor"": 0.78868761835093038,
        ""id"": 753,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/22""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/760"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Unique Panda has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/760/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/759"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 759
    },
    ""id"": 760,
    ""name"": ""Unique Panda"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/757"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/760""
        },
        ""id"": 757,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/12""
        },
        ""sku"": ""1551"",
        ""startsOn"": ""2012-10-08T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/758"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/760""
        },
        ""id"": 758,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""sku"": ""8262"",
        ""startsOn"": ""2012-09-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/756"",
        ""dependability"": 0.098170928237108018,
        ""id"": 756,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/44""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/767"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Hurtful Bear has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/767/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/766"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 766
    },
    ""id"": 767,
    ""instrument"": ""Few Bass"",
    ""name"": ""Hurtful Bear"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/764"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/767"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 764,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""sku"": ""5222"",
        ""startsOn"": ""2012-09-24T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/765"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/767"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 765,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""sku"": ""6733"",
        ""startsOn"": ""2012-11-01T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/761"",
        ""_type"": ""Gun"",
        ""dependability"": 0.49749497952754373,
        ""explosionFactor"": 0.7274063987319388,
        ""id"": 761,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/762"",
        ""_type"": ""Gun"",
        ""dependability"": 0.1767307949144071,
        ""explosionFactor"": 0.12396969977951129,
        ""id"": 762,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/763"",
        ""dependability"": 0.79791443506158632,
        ""id"": 763,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/776"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Bad Coati has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/776/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/775"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 775
    },
    ""id"": 776,
    ""instrument"": ""Warmhearted Drum"",
    ""name"": ""Bad Coati"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/772"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/776"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 772,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        },
        ""sku"": ""408"",
        ""startsOn"": ""2012-09-29T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/773"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/776"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 773,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        },
        ""sku"": ""5401"",
        ""startsOn"": ""2012-10-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/774"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/776"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 774,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""4969"",
        ""startsOn"": ""2012-08-13T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/768"",
        ""_type"": ""Gun"",
        ""dependability"": 0.55073238981456141,
        ""explosionFactor"": 0.041606111005696521,
        ""id"": 768,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/769"",
        ""_type"": ""Gun"",
        ""dependability"": 0.38618655753610032,
        ""explosionFactor"": 0.37181589164390039,
        ""id"": 769,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/770"",
        ""dependability"": 0.73556466853970881,
        ""id"": 770,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/771"",
        ""_type"": ""Gun"",
        ""dependability"": 0.6224148001626203,
        ""explosionFactor"": 0.65856622981772117,
        ""id"": 771,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/785"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Knotty Dugong has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/785/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/784"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 784
    },
    ""id"": 785,
    ""name"": ""Knotty Dugong"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/781"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/785""
        },
        ""id"": 781,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""3561"",
        ""startsOn"": ""2012-08-18T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/782"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/785""
        },
        ""id"": 782,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""206"",
        ""startsOn"": ""2012-09-09T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/783"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/785""
        },
        ""id"": 783,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""sku"": ""5228"",
        ""startsOn"": ""2012-08-24T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/777"",
        ""_type"": ""Gun"",
        ""dependability"": 0.43929608000409609,
        ""explosionFactor"": 0.87999152107163869,
        ""id"": 777,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/778"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62599840509984572,
        ""explosionFactor"": 0.082363588773814764,
        ""id"": 778,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/779"",
        ""dependability"": 0.411727877525486,
        ""id"": 779,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/780"",
        ""dependability"": 0.67947301020821227,
        ""id"": 780,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/792"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Dopey Mule has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/792/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/791"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 791
    },
    ""id"": 792,
    ""name"": ""Dopey Mule"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/790"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/792""
        },
        ""id"": 790,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        },
        ""sku"": ""9112"",
        ""startsOn"": ""2012-08-24T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/786"",
        ""dependability"": 0.91801685463544769,
        ""id"": 786,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/787"",
        ""_type"": ""Gun"",
        ""dependability"": 0.16868031731279581,
        ""explosionFactor"": 0.64032022126033916,
        ""id"": 787,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/788"",
        ""_type"": ""Gun"",
        ""dependability"": 0.94494555562033578,
        ""explosionFactor"": 0.65455560323528739,
        ""id"": 788,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/789"",
        ""_type"": ""Gun"",
        ""dependability"": 0.010335365315124096,
        ""explosionFactor"": 0.98195785143503822,
        ""id"": 789,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/797"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Aching Doe has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/797/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/796"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 796
    },
    ""id"": 797,
    ""name"": ""Aching Doe"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/794"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/797""
        },
        ""id"": 794,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""5726"",
        ""startsOn"": ""2012-08-02T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/795"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/797""
        },
        ""id"": 795,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""5722"",
        ""startsOn"": ""2012-08-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/793"",
        ""dependability"": 0.52462210856593317,
        ""id"": 793,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/801"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Loathsome Prairie has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/801/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/800"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 800
    },
    ""id"": 801,
    ""name"": ""Loathsome Prairie"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/798"",
        ""_type"": ""Gun"",
        ""dependability"": 0.1133949864252447,
        ""explosionFactor"": 0.91889247480728309,
        ""id"": 798,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/799"",
        ""dependability"": 0.72518698485809707,
        ""id"": 799,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/808"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tame Lizard has tumour""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/808/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/807"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 807
    },
    ""id"": 808,
    ""name"": ""Tame Lizard"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/805"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/808""
        },
        ""id"": 805,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        },
        ""sku"": ""8919"",
        ""startsOn"": ""2012-11-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/806"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/808""
        },
        ""id"": 806,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        },
        ""sku"": ""1107"",
        ""startsOn"": ""2012-10-21T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/802"",
        ""_type"": ""Gun"",
        ""dependability"": 0.65224807600129775,
        ""explosionFactor"": 0.73252767684521514,
        ""id"": 802,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/803"",
        ""dependability"": 0.86950424121204029,
        ""id"": 803,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/804"",
        ""dependability"": 0.0679521831068919,
        ""id"": 804,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/816"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Small Tiger has cellulitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/816/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/815"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 815
    },
    ""id"": 816,
    ""name"": ""Small Tiger"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/812"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/816""
        },
        ""id"": 812,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        },
        ""sku"": ""5358"",
        ""startsOn"": ""2012-10-01T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/813"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/816""
        },
        ""id"": 813,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""sku"": ""7680"",
        ""startsOn"": ""2012-10-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/814"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/816""
        },
        ""id"": 814,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        },
        ""sku"": ""8666"",
        ""startsOn"": ""2012-08-01T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/809"",
        ""_type"": ""Gun"",
        ""dependability"": 0.088900935411872772,
        ""explosionFactor"": 0.84213307958195593,
        ""id"": 809,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/43""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/810"",
        ""_type"": ""Gun"",
        ""dependability"": 0.52230150928828,
        ""explosionFactor"": 0.38541086129164831,
        ""id"": 810,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/811"",
        ""_type"": ""Gun"",
        ""dependability"": 0.61342451610296245,
        ""explosionFactor"": 0.33982007640405559,
        ""id"": 811,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/819"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Granular Ass has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/819/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/818"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 818
    },
    ""id"": 819,
    ""name"": ""Granular Ass"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/817"",
        ""_type"": ""Gun"",
        ""dependability"": 0.32602891667095429,
        ""explosionFactor"": 0.92133606873514884,
        ""id"": 817,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/824"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Respectful Doe has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/824/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/823"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 823
    },
    ""id"": 824,
    ""name"": ""Respectful Doe"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/820"",
        ""_type"": ""Gun"",
        ""dependability"": 0.85384975739468338,
        ""explosionFactor"": 0.23379714937591792,
        ""id"": 820,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/60""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/821"",
        ""_type"": ""Gun"",
        ""dependability"": 0.22765198220855182,
        ""explosionFactor"": 0.25107425788933146,
        ""id"": 821,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/822"",
        ""_type"": ""Gun"",
        ""dependability"": 0.46567633816305376,
        ""explosionFactor"": 0.57485281050896864,
        ""id"": 822,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/832"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Authorized Capybara has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/832/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/831"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 831
    },
    ""id"": 832,
    ""name"": ""Authorized Capybara"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/829"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/832""
        },
        ""id"": 829,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""sku"": ""9139"",
        ""startsOn"": ""2012-08-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/830"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/832""
        },
        ""id"": 830,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        },
        ""sku"": ""8584"",
        ""startsOn"": ""2012-09-20T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/825"",
        ""dependability"": 0.03450723459688352,
        ""id"": 825,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/826"",
        ""dependability"": 0.01360136457420949,
        ""id"": 826,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/827"",
        ""dependability"": 0.70542056379160867,
        ""id"": 827,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/828"",
        ""_type"": ""Gun"",
        ""dependability"": 0.083362430372909843,
        ""explosionFactor"": 0.58033265852384863,
        ""id"": 828,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/840"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Flat Alligator has amnesia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/840/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/839"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 839
    },
    ""id"": 840,
    ""name"": ""Flat Alligator"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/837"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/840""
        },
        ""id"": 837,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        },
        ""sku"": ""3310"",
        ""startsOn"": ""2012-10-29T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/838"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/840""
        },
        ""id"": 838,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        },
        ""sku"": ""4687"",
        ""startsOn"": ""2012-09-20T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/833"",
        ""dependability"": 0.38426452147972978,
        ""id"": 833,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/834"",
        ""_type"": ""Gun"",
        ""dependability"": 0.77686384263302377,
        ""explosionFactor"": 0.13971865230226827,
        ""id"": 834,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/835"",
        ""_type"": ""Gun"",
        ""dependability"": 0.12286199355631228,
        ""explosionFactor"": 0.91557703675496249,
        ""id"": 835,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/836"",
        ""_type"": ""Gun"",
        ""dependability"": 0.34158741605542015,
        ""explosionFactor"": 0.84024839282047392,
        ""id"": 836,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/845"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Mindless Weasel has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/845/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/844"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 844
    },
    ""id"": 845,
    ""instrument"": ""New Contrabassoon"",
    ""name"": ""Mindless Weasel"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/843"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/845"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 843,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/12""
        },
        ""sku"": ""7097"",
        ""startsOn"": ""2012-09-28T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/841"",
        ""dependability"": 0.315173013748216,
        ""id"": 841,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/842"",
        ""dependability"": 0.096559310376904586,
        ""id"": 842,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/16""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/851"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Official Elephant has gastroentroitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/851/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/850"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 850
    },
    ""id"": 851,
    ""name"": ""Official Elephant"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/848"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/851""
        },
        ""id"": 848,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        },
        ""sku"": ""9722"",
        ""startsOn"": ""2012-09-04T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/849"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/851""
        },
        ""id"": 849,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        },
        ""sku"": ""6342"",
        ""startsOn"": ""2012-11-02T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/846"",
        ""_type"": ""Gun"",
        ""dependability"": 0.83072434357866842,
        ""explosionFactor"": 0.27209409711514326,
        ""id"": 846,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/847"",
        ""dependability"": 0.31580425580768112,
        ""id"": 847,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/855"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Equatorial Crow has tumour""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/855/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/854"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 854
    },
    ""id"": 855,
    ""name"": ""Equatorial Crow"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/853"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/855""
        },
        ""id"": 853,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""2450"",
        ""startsOn"": ""2012-09-08T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/852"",
        ""dependability"": 0.60040402300674656,
        ""id"": 852,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/863"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Stark Mink has acidosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/863/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/862"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 862
    },
    ""id"": 863,
    ""name"": ""Stark Mink"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/859"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/863""
        },
        ""id"": 859,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/18""
        },
        ""sku"": ""3630"",
        ""startsOn"": ""2012-11-05T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/860"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/863""
        },
        ""id"": 860,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        },
        ""sku"": ""3572"",
        ""startsOn"": ""2012-10-19T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/861"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/863""
        },
        ""id"": 861,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        },
        ""sku"": ""4208"",
        ""startsOn"": ""2012-09-05T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/856"",
        ""_type"": ""Gun"",
        ""dependability"": 0.46140636990843636,
        ""explosionFactor"": 0.46598109857457742,
        ""id"": 856,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/857"",
        ""_type"": ""Gun"",
        ""dependability"": 0.25869661581641834,
        ""explosionFactor"": 0.17673557492752354,
        ""id"": 857,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/858"",
        ""dependability"": 0.87525862589257708,
        ""id"": 858,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/34""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/872"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Trusty Bear has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/872/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/871"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 871
    },
    ""id"": 872,
    ""name"": ""Trusty Bear"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/868"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/872""
        },
        ""id"": 868,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        },
        ""sku"": ""1809"",
        ""startsOn"": ""2012-10-17T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/869"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/872""
        },
        ""id"": 869,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""1190"",
        ""startsOn"": ""2012-09-02T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/870"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/872""
        },
        ""id"": 870,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""5200"",
        ""startsOn"": ""2012-08-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/864"",
        ""_type"": ""Gun"",
        ""dependability"": 0.6552842634987478,
        ""explosionFactor"": 0.30250245113973617,
        ""id"": 864,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/865"",
        ""dependability"": 0.39231503307461507,
        ""id"": 865,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/866"",
        ""_type"": ""Gun"",
        ""dependability"": 0.99647249281242145,
        ""explosionFactor"": 0.17286005391406828,
        ""id"": 866,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/867"",
        ""_type"": ""Gun"",
        ""dependability"": 0.10367118385791369,
        ""explosionFactor"": 0.98737651388504377,
        ""id"": 867,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/880"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Lucky Coyote has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/880/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/879"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 879
    },
    ""id"": 880,
    ""instrument"": ""Lively Saxophone"",
    ""name"": ""Lucky Coyote"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/876"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/880"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 876,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        },
        ""sku"": ""16"",
        ""startsOn"": ""2012-10-11T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/877"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/880"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 877,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""4335"",
        ""startsOn"": ""2012-10-03T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/878"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/880"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 878,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""9017"",
        ""startsOn"": ""2012-08-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/873"",
        ""_type"": ""Gun"",
        ""dependability"": 0.96681567140240954,
        ""explosionFactor"": 0.23163062437979068,
        ""id"": 873,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/874"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2065575207614142,
        ""explosionFactor"": 0.99278038367292865,
        ""id"": 874,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/875"",
        ""dependability"": 0.91716082204001059,
        ""id"": 875,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/885"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Taut Raccoon has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/885/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/884"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 884
    },
    ""id"": 885,
    ""name"": ""Taut Raccoon"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/882"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/885""
        },
        ""id"": 882,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        },
        ""sku"": ""9418"",
        ""startsOn"": ""2012-11-02T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/883"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/885""
        },
        ""id"": 883,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        },
        ""sku"": ""3858"",
        ""startsOn"": ""2012-08-01T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/881"",
        ""dependability"": 0.390252368706396,
        ""id"": 881,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/891"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Essential Pony has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/891/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/890"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 890
    },
    ""id"": 891,
    ""name"": ""Essential Pony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/886"",
        ""_type"": ""Gun"",
        ""dependability"": 0.28202752875258563,
        ""explosionFactor"": 0.39739321097610203,
        ""id"": 886,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/887"",
        ""dependability"": 0.1327811619885178,
        ""id"": 887,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/888"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2623105096920908,
        ""explosionFactor"": 0.64709665190758958,
        ""id"": 888,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/889"",
        ""_type"": ""Gun"",
        ""dependability"": 0.40051668994152767,
        ""explosionFactor"": 0.26782844973161279,
        ""id"": 889,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/897"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Helpless Mountain has shingles""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/897/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/896"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 896
    },
    ""id"": 897,
    ""name"": ""Helpless Mountain"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/894"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/897""
        },
        ""id"": 894,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""sku"": ""6777"",
        ""startsOn"": ""2012-09-03T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/895"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/897""
        },
        ""id"": 895,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""sku"": ""9273"",
        ""startsOn"": ""2012-09-23T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/892"",
        ""dependability"": 0.63016711996410368,
        ""id"": 892,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/893"",
        ""_type"": ""Gun"",
        ""dependability"": 0.08319940794408294,
        ""explosionFactor"": 0.61571138101430212,
        ""id"": 893,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/903"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Intent Lemur has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/903/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/902"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 902
    },
    ""id"": 903,
    ""name"": ""Intent Lemur"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/900"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/903""
        },
        ""id"": 900,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        },
        ""sku"": ""1180"",
        ""startsOn"": ""2012-11-04T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/901"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/903""
        },
        ""id"": 901,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""sku"": ""1481"",
        ""startsOn"": ""2012-11-07T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/898"",
        ""dependability"": 0.10199558972473982,
        ""id"": 898,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/899"",
        ""dependability"": 0.11174449702340387,
        ""id"": 899,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/911"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Broken Mouse has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/911/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/910"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 910
    },
    ""id"": 911,
    ""name"": ""Broken Mouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/907"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/911""
        },
        ""id"": 907,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        },
        ""sku"": ""6828"",
        ""startsOn"": ""2012-10-17T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/908"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/911""
        },
        ""id"": 908,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        },
        ""sku"": ""1009"",
        ""startsOn"": ""2012-11-23T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/909"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/911""
        },
        ""id"": 909,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        },
        ""sku"": ""5004"",
        ""startsOn"": ""2012-08-29T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/904"",
        ""dependability"": 0.766768135953121,
        ""id"": 904,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/905"",
        ""dependability"": 0.22114319690556414,
        ""id"": 905,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/906"",
        ""_type"": ""Gun"",
        ""dependability"": 0.66589745770483155,
        ""explosionFactor"": 0.34584273600291587,
        ""id"": 906,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/918"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Late Silver has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/918/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/917"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 917
    },
    ""id"": 918,
    ""name"": ""Late Silver"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/914"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/918""
        },
        ""id"": 914,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""sku"": ""5335"",
        ""startsOn"": ""2012-09-27T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/915"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/918""
        },
        ""id"": 915,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        },
        ""sku"": ""9558"",
        ""startsOn"": ""2012-11-09T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/916"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/918""
        },
        ""id"": 916,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/59""
        },
        ""sku"": ""2339"",
        ""startsOn"": ""2012-09-18T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/912"",
        ""_type"": ""Gun"",
        ""dependability"": 0.52960053576603561,
        ""explosionFactor"": 0.46102912512655797,
        ""id"": 912,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/913"",
        ""_type"": ""Gun"",
        ""dependability"": 0.39353275317397562,
        ""explosionFactor"": 0.65805233626535742,
        ""id"": 913,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/924"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Dazzling Dugong has deafness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/924/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/923"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 923
    },
    ""id"": 924,
    ""name"": ""Dazzling Dugong"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/921"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/924""
        },
        ""id"": 921,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        },
        ""sku"": ""567"",
        ""startsOn"": ""2012-11-08T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/922"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/924""
        },
        ""id"": 922,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""sku"": ""9028"",
        ""startsOn"": ""2012-08-15T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/919"",
        ""dependability"": 0.48786539653682404,
        ""id"": 919,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/920"",
        ""dependability"": 0.38012064033193543,
        ""id"": 920,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/932"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Worn Silver has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/932/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/931"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 931
    },
    ""id"": 932,
    ""name"": ""Worn Silver"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/928"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/932""
        },
        ""id"": 928,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""2741"",
        ""startsOn"": ""2012-10-09T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/929"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/932""
        },
        ""id"": 929,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/22""
        },
        ""sku"": ""5945"",
        ""startsOn"": ""2012-10-23T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/930"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/932""
        },
        ""id"": 930,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""7016"",
        ""startsOn"": ""2012-08-19T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/925"",
        ""dependability"": 0.66816717231095168,
        ""id"": 925,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/926"",
        ""dependability"": 0.47491758478568752,
        ""id"": 926,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/927"",
        ""dependability"": 0.43351977245580392,
        ""id"": 927,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/937"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Favorable Rabbit has imperteigo""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/937/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/936"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 936
    },
    ""id"": 937,
    ""name"": ""Favorable Rabbit"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/935"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/937""
        },
        ""id"": 935,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/32""
        },
        ""sku"": ""1631"",
        ""startsOn"": ""2012-10-25T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/933"",
        ""dependability"": 0.49879460944737986,
        ""id"": 933,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/934"",
        ""dependability"": 0.79473220826812652,
        ""id"": 934,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/940"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Decent Monkey has shingles""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/940/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/939"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 939
    },
    ""id"": 940,
    ""instrument"": ""Gorgeous Ondes-martenot"",
    ""name"": ""Decent Monkey"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/938"",
        ""dependability"": 0.7451247376134269,
        ""id"": 938,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/946"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Required Dog has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/946/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/945"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 945
    },
    ""id"": 946,
    ""name"": ""Required Dog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/943"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/946""
        },
        ""id"": 943,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/65""
        },
        ""sku"": ""9760"",
        ""startsOn"": ""2012-10-23T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/944"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/946""
        },
        ""id"": 944,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""6387"",
        ""startsOn"": ""2012-09-21T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/941"",
        ""_type"": ""Gun"",
        ""dependability"": 0.7872219494484467,
        ""explosionFactor"": 0.6083822821305982,
        ""id"": 941,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/942"",
        ""_type"": ""Gun"",
        ""dependability"": 0.51646465878769043,
        ""explosionFactor"": 0.20788546847546727,
        ""id"": 942,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/952"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Esteemed Hog has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/952/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/951"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 951
    },
    ""id"": 952,
    ""instrument"": ""Careless Trombone"",
    ""name"": ""Esteemed Hog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/949"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/952"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 949,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""8052"",
        ""startsOn"": ""2012-10-24T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/950"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/952"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 950,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        },
        ""sku"": ""6620"",
        ""startsOn"": ""2012-08-20T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/947"",
        ""_type"": ""Gun"",
        ""dependability"": 0.34258893706956361,
        ""explosionFactor"": 0.82489373806160582,
        ""id"": 947,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/948"",
        ""dependability"": 0.31922347299718462,
        ""id"": 948,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/960"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Anguished Lynx has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/960/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/959"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 959
    },
    ""id"": 960,
    ""name"": ""Anguished Lynx"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/957"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/960""
        },
        ""id"": 957,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        },
        ""sku"": ""2657"",
        ""startsOn"": ""2012-11-11T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/958"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/960""
        },
        ""id"": 958,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""9690"",
        ""startsOn"": ""2012-11-10T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/953"",
        ""_type"": ""Gun"",
        ""dependability"": 0.24280860379469049,
        ""explosionFactor"": 0.036824836412828806,
        ""id"": 953,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/954"",
        ""_type"": ""Gun"",
        ""dependability"": 0.30781330368845411,
        ""explosionFactor"": 0.27796109918410011,
        ""id"": 954,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/955"",
        ""dependability"": 0.37982676149337868,
        ""id"": 955,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/956"",
        ""_type"": ""Gun"",
        ""dependability"": 0.34166552095751535,
        ""explosionFactor"": 0.81049879119289048,
        ""id"": 956,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/965"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Parched Zebra has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/965/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/964"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 964
    },
    ""id"": 965,
    ""instrument"": ""Minor Clarinet"",
    ""name"": ""Parched Zebra"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/963"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/965"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 963,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""sku"": ""9897"",
        ""startsOn"": ""2012-10-25T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/961"",
        ""dependability"": 0.83145181407753932,
        ""id"": 961,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/962"",
        ""_type"": ""Gun"",
        ""dependability"": 0.63845247525649729,
        ""explosionFactor"": 0.25250550464377997,
        ""id"": 962,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/971"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Colossal Duckbill has shingles""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/971/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/970"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 970
    },
    ""id"": 971,
    ""name"": ""Colossal Duckbill"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/967"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/971""
        },
        ""id"": 967,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        },
        ""sku"": ""712"",
        ""startsOn"": ""2012-08-30T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/968"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/971""
        },
        ""id"": 968,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""sku"": ""1934"",
        ""startsOn"": ""2012-11-07T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/969"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/971""
        },
        ""id"": 969,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        },
        ""sku"": ""2334"",
        ""startsOn"": ""2012-10-20T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/966"",
        ""dependability"": 0.95108182074086822,
        ""id"": 966,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/976"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Afraid Frog has dehydration""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/976/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/975"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 975
    },
    ""id"": 976,
    ""instrument"": ""Growing Harmonica"",
    ""name"": ""Afraid Frog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/973"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/976"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 973,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/39""
        },
        ""sku"": ""3558"",
        ""startsOn"": ""2012-09-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/974"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/976"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 974,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        },
        ""sku"": ""1426"",
        ""startsOn"": ""2012-10-13T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/972"",
        ""dependability"": 0.64396539593300106,
        ""id"": 972,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/981"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Stupendous Pony has asthma""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/981/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/980"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 980
    },
    ""id"": 981,
    ""name"": ""Stupendous Pony"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/977"",
        ""dependability"": 0.12513588933513309,
        ""id"": 977,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/978"",
        ""_type"": ""Gun"",
        ""dependability"": 0.2672848958835401,
        ""explosionFactor"": 0.68999930084217309,
        ""id"": 978,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/979"",
        ""dependability"": 0.081731691994579359,
        ""id"": 979,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/26""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/989"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Pleased Eland has imperteigo""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/989/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/988"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 988
    },
    ""id"": 989,
    ""name"": ""Pleased Eland"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/985"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/989""
        },
        ""id"": 985,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/37""
        },
        ""sku"": ""1618"",
        ""startsOn"": ""2012-10-31T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/986"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/989""
        },
        ""id"": 986,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        },
        ""sku"": ""1989"",
        ""startsOn"": ""2012-08-26T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/987"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/989""
        },
        ""id"": 987,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""sku"": ""8066"",
        ""startsOn"": ""2012-08-20T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/982"",
        ""_type"": ""Gun"",
        ""dependability"": 0.90753420158640208,
        ""explosionFactor"": 0.4731071332810014,
        ""id"": 982,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/17""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/983"",
        ""_type"": ""Gun"",
        ""dependability"": 0.24786877503985016,
        ""explosionFactor"": 0.29358315528071632,
        ""id"": 983,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/984"",
        ""dependability"": 0.013039322576038223,
        ""id"": 984,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/14""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/996"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""These Ox has burn""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/996/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/995"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 995
    },
    ""id"": 996,
    ""name"": ""These Ox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/992"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/996""
        },
        ""id"": 992,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""5316"",
        ""startsOn"": ""2012-11-12T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/993"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/996""
        },
        ""id"": 993,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""sku"": ""7412"",
        ""startsOn"": ""2012-08-27T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/994"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/996""
        },
        ""id"": 994,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""8314"",
        ""startsOn"": ""2012-09-22T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/990"",
        ""dependability"": 0.53897333030541117,
        ""id"": 990,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/991"",
        ""_type"": ""Gun"",
        ""dependability"": 0.64835927153395456,
        ""explosionFactor"": 0.85516294364592194,
        ""id"": 991,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1001"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Remorseful Dormouse has burn""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1001/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1000"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1000
    },
    ""id"": 1001,
    ""name"": ""Remorseful Dormouse"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/999"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1001""
        },
        ""id"": 999,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""5232"",
        ""startsOn"": ""2012-08-03T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/997"",
        ""dependability"": 0.37304530077290038,
        ""id"": 997,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/998"",
        ""dependability"": 0.48072291281107948,
        ""id"": 998,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1009"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Weekly Marmoset has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1009/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1008"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1008
    },
    ""id"": 1009,
    ""name"": ""Weekly Marmoset"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1006"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1009""
        },
        ""id"": 1006,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""sku"": ""4643"",
        ""startsOn"": ""2012-10-18T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1007"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1009""
        },
        ""id"": 1007,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""9399"",
        ""startsOn"": ""2012-09-14T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1002"",
        ""_type"": ""Gun"",
        ""dependability"": 0.36468005104208367,
        ""explosionFactor"": 0.14364346495999184,
        ""id"": 1002,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1003"",
        ""dependability"": 0.26889670745883915,
        ""id"": 1003,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1004"",
        ""_type"": ""Gun"",
        ""dependability"": 0.5088760072872397,
        ""explosionFactor"": 0.789181327349125,
        ""id"": 1004,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/35""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1005"",
        ""_type"": ""Gun"",
        ""dependability"": 0.84353708189145526,
        ""explosionFactor"": 0.10597192128466998,
        ""id"": 1005,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/27""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1015"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Hard-to-find Bat has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1015/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1014"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1014
    },
    ""id"": 1015,
    ""name"": ""Hard-to-find Bat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1013"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1015""
        },
        ""id"": 1013,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""121"",
        ""startsOn"": ""2012-08-06T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1010"",
        ""dependability"": 0.42333695265619875,
        ""id"": 1010,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1011"",
        ""_type"": ""Gun"",
        ""dependability"": 0.78334655695750688,
        ""explosionFactor"": 0.07694290907911161,
        ""id"": 1011,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1012"",
        ""_type"": ""Gun"",
        ""dependability"": 0.88956552226541818,
        ""explosionFactor"": 0.54754942727626743,
        ""id"": 1012,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1018"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Soupy Lamb has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1018/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1017"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1017
    },
    ""id"": 1018,
    ""name"": ""Soupy Lamb"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1016"",
        ""_type"": ""Gun"",
        ""dependability"": 0.46588424661470773,
        ""explosionFactor"": 0.44754873236992804,
        ""id"": 1016,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1023"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Stupid Elk has shingles""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1023/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1022"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1022
    },
    ""id"": 1023,
    ""name"": ""Stupid Elk"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1021"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1023""
        },
        ""id"": 1021,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        },
        ""sku"": ""6136"",
        ""startsOn"": ""2012-08-31T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1019"",
        ""dependability"": 0.097735104662242861,
        ""id"": 1019,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1020"",
        ""_type"": ""Gun"",
        ""dependability"": 0.6679395808223354,
        ""explosionFactor"": 0.73756998578904664,
        ""id"": 1020,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1028"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Rotating Waterbuck has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1028/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1027"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1027
    },
    ""id"": 1028,
    ""name"": ""Rotating Waterbuck"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1025"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1028""
        },
        ""id"": 1025,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/20""
        },
        ""sku"": ""2907"",
        ""startsOn"": ""2012-09-08T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1026"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1028""
        },
        ""id"": 1026,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        },
        ""sku"": ""5910"",
        ""startsOn"": ""2012-09-10T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1024"",
        ""_type"": ""Gun"",
        ""dependability"": 0.85532318700818488,
        ""explosionFactor"": 0.19867144534302478,
        ""id"": 1024,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1037"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Extra-small Kid has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1037/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1036"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1036
    },
    ""id"": 1037,
    ""instrument"": ""Barren Drum"",
    ""name"": ""Extra-small Kid"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1033"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1037"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1033,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""2398"",
        ""startsOn"": ""2012-11-18T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1034"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1037"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1034,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        },
        ""sku"": ""8188"",
        ""startsOn"": ""2012-10-13T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1035"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1037"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1035,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/69""
        },
        ""sku"": ""4784"",
        ""startsOn"": ""2012-10-22T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1029"",
        ""dependability"": 0.16675871851237431,
        ""id"": 1029,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1030"",
        ""dependability"": 0.41677588523215425,
        ""id"": 1030,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1031"",
        ""_type"": ""Gun"",
        ""dependability"": 0.30871522534113155,
        ""explosionFactor"": 0.470016930005521,
        ""id"": 1031,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/33""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1032"",
        ""_type"": ""Gun"",
        ""dependability"": 0.47390955242976057,
        ""explosionFactor"": 0.76174757804802973,
        ""id"": 1032,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1043"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""First Ocelot has insomnia""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1043/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1042"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1042
    },
    ""id"": 1043,
    ""name"": ""First Ocelot"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1040"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1043""
        },
        ""id"": 1040,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""4385"",
        ""startsOn"": ""2012-10-21T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1041"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1043""
        },
        ""id"": 1041,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        },
        ""sku"": ""9850"",
        ""startsOn"": ""2012-11-02T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1038"",
        ""_type"": ""Gun"",
        ""dependability"": 0.035097702888351727,
        ""explosionFactor"": 0.75675122428534147,
        ""id"": 1038,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/29""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1039"",
        ""_type"": ""Gun"",
        ""dependability"": 0.84128733391002164,
        ""explosionFactor"": 0.033184271321252116,
        ""id"": 1039,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1047"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Cheery Capybara has cholera""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1047/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1046"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1046
    },
    ""id"": 1047,
    ""name"": ""Cheery Capybara"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1045"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1047""
        },
        ""id"": 1045,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        },
        ""sku"": ""2276"",
        ""startsOn"": ""2012-10-07T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1044"",
        ""dependability"": 0.53953023792222621,
        ""id"": 1044,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1051"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Scared Hedgehog has tuberculosis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1051/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1050"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1050
    },
    ""id"": 1051,
    ""name"": ""Scared Hedgehog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1049"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1051""
        },
        ""id"": 1049,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/54""
        },
        ""sku"": ""5366"",
        ""startsOn"": ""2012-09-14T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1048"",
        ""dependability"": 0.89382588253069006,
        ""id"": 1048,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1057"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Trusting Yak has chancroid""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1057/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1056"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1056
    },
    ""id"": 1057,
    ""name"": ""Trusting Yak"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1054"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1057""
        },
        ""id"": 1054,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/42""
        },
        ""sku"": ""1183"",
        ""startsOn"": ""2012-10-06T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1055"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1057""
        },
        ""id"": 1055,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""7319"",
        ""startsOn"": ""2012-09-29T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1052"",
        ""_type"": ""Gun"",
        ""dependability"": 0.20464230710856723,
        ""explosionFactor"": 0.00925508793874415,
        ""id"": 1052,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1053"",
        ""_type"": ""Gun"",
        ""dependability"": 0.62049446283862664,
        ""explosionFactor"": 0.55211290975665339,
        ""id"": 1053,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/46""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1063"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Striped Monkey has tonsilitus""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1063/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1062"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1062
    },
    ""id"": 1063,
    ""instrument"": ""Long Mandolin"",
    ""name"": ""Striped Monkey"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1060"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1063"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1060,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/67""
        },
        ""sku"": ""1612"",
        ""startsOn"": ""2012-08-29T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1061"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1063"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1061,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""sku"": ""1581"",
        ""startsOn"": ""2012-10-28T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1058"",
        ""dependability"": 0.091279378669000877,
        ""id"": 1058,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1059"",
        ""dependability"": 0.60417279442966576,
        ""id"": 1059,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/32""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1070"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Outlying Anteater has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1070/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1069"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1069
    },
    ""id"": 1070,
    ""name"": ""Outlying Anteater"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1066"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1070""
        },
        ""id"": 1066,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""sku"": ""9954"",
        ""startsOn"": ""2012-10-04T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1067"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1070""
        },
        ""id"": 1067,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""sku"": ""4451"",
        ""startsOn"": ""2012-08-28T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1068"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1070""
        },
        ""id"": 1068,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/62""
        },
        ""sku"": ""8872"",
        ""startsOn"": ""2012-08-21T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1064"",
        ""dependability"": 0.46033529679306562,
        ""id"": 1064,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1065"",
        ""dependability"": 0.29812964112410772,
        ""id"": 1065,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1075"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Utter Newt has bronchitis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1075/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1074"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1074
    },
    ""id"": 1075,
    ""name"": ""Utter Newt"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1073"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1075""
        },
        ""id"": 1073,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/66""
        },
        ""sku"": ""4201"",
        ""startsOn"": ""2012-11-01T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1071"",
        ""dependability"": 0.7092441817322952,
        ""id"": 1071,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1072"",
        ""dependability"": 0.48348388424305427,
        ""id"": 1072,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/31""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1082"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Whirlwind Panda has burn""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1082/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1081"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1081
    },
    ""id"": 1082,
    ""instrument"": ""Unruly Dynamophone"",
    ""name"": ""Whirlwind Panda"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1080"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1082"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1080,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/47""
        },
        ""sku"": ""4514"",
        ""startsOn"": ""2012-11-04T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1076"",
        ""dependability"": 0.69016671445694133,
        ""id"": 1076,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/52""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1077"",
        ""dependability"": 0.13116495922727742,
        ""id"": 1077,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1078"",
        ""dependability"": 0.0090804342222774552,
        ""id"": 1078,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1079"",
        ""_type"": ""Gun"",
        ""dependability"": 0.57103799962021318,
        ""explosionFactor"": 0.037355399707963408,
        ""id"": 1079,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1090"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Sick Orangutan has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1090/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1089"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1089
    },
    ""id"": 1090,
    ""name"": ""Sick Orangutan"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1087"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1090""
        },
        ""id"": 1087,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/48""
        },
        ""sku"": ""4076"",
        ""startsOn"": ""2012-10-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1088"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1090""
        },
        ""id"": 1088,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/63""
        },
        ""sku"": ""7365"",
        ""startsOn"": ""2012-11-03T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1083"",
        ""_type"": ""Gun"",
        ""dependability"": 0.097947144460839755,
        ""explosionFactor"": 0.57536145000502537,
        ""id"": 1083,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/57""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1084"",
        ""_type"": ""Gun"",
        ""dependability"": 0.54597765605243742,
        ""explosionFactor"": 0.92574213022633556,
        ""id"": 1084,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1085"",
        ""dependability"": 0.36240475269146483,
        ""id"": 1085,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/13""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1086"",
        ""dependability"": 0.12482597172485012,
        ""id"": 1086,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1097"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Hasty Cat has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1097/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1096"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1096
    },
    ""id"": 1097,
    ""name"": ""Hasty Cat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1093"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1097""
        },
        ""id"": 1093,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""6252"",
        ""startsOn"": ""2012-07-30T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1094"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1097""
        },
        ""id"": 1094,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        },
        ""sku"": ""5310"",
        ""startsOn"": ""2012-08-16T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1095"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1097""
        },
        ""id"": 1095,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""sku"": ""292"",
        ""startsOn"": ""2012-09-06T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1091"",
        ""_type"": ""Gun"",
        ""dependability"": 0.509483822858652,
        ""explosionFactor"": 0.62409419967983581,
        ""id"": 1091,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1092"",
        ""_type"": ""Gun"",
        ""dependability"": 0.266225343694084,
        ""explosionFactor"": 0.79989322684700281,
        ""id"": 1092,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/19""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1105"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Jagged Fox has epilepsy""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1105/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1104"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1104
    },
    ""id"": 1105,
    ""name"": ""Jagged Fox"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1102"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1105""
        },
        ""id"": 1102,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/45""
        },
        ""sku"": ""2644"",
        ""startsOn"": ""2012-11-12T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1103"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1105""
        },
        ""id"": 1103,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""sku"": ""1463"",
        ""startsOn"": ""2012-08-16T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1098"",
        ""dependability"": 0.97016335649889118,
        ""id"": 1098,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1099"",
        ""dependability"": 0.12705032300532346,
        ""id"": 1099,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/58""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1100"",
        ""_type"": ""Gun"",
        ""dependability"": 0.71943426258835674,
        ""explosionFactor"": 0.19249807586544104,
        ""id"": 1100,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/38""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1101"",
        ""_type"": ""Gun"",
        ""dependability"": 0.098191710700370236,
        ""explosionFactor"": 0.41625830736768354,
        ""id"": 1101,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1111"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Good-natured Gorilla has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1111/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1110"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1110
    },
    ""id"": 1111,
    ""name"": ""Good-natured Gorilla"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1109"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1111""
        },
        ""id"": 1109,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/24""
        },
        ""sku"": ""6727"",
        ""startsOn"": ""2012-09-16T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1106"",
        ""_type"": ""Gun"",
        ""dependability"": 0.80070094987782692,
        ""explosionFactor"": 0.16987963494373468,
        ""id"": 1106,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1107"",
        ""_type"": ""Gun"",
        ""dependability"": 0.663197286270185,
        ""explosionFactor"": 0.49684542347530153,
        ""id"": 1107,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/36""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1108"",
        ""dependability"": 0.30889438060526475,
        ""id"": 1108,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/25""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1116"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Tempting Steer has cancer""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1116/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1115"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1115
    },
    ""id"": 1116,
    ""name"": ""Tempting Steer"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1114"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1116""
        },
        ""id"": 1114,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        },
        ""sku"": ""7822"",
        ""startsOn"": ""2012-08-06T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1112"",
        ""dependability"": 0.46658250292138315,
        ""id"": 1112,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1113"",
        ""dependability"": 0.961454875749282,
        ""id"": 1113,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1122"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Joyous Mule has fever.""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1122/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1121"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1121
    },
    ""id"": 1122,
    ""name"": ""Joyous Mule"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1119"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1122""
        },
        ""id"": 1119,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        },
        ""sku"": ""48"",
        ""startsOn"": ""2012-09-30T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1120"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1122""
        },
        ""id"": 1120,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/30""
        },
        ""sku"": ""7150"",
        ""startsOn"": ""2012-08-01T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1117"",
        ""_type"": ""Gun"",
        ""dependability"": 0.549554627644622,
        ""explosionFactor"": 0.67592716760743743,
        ""id"": 1117,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1118"",
        ""_type"": ""Gun"",
        ""dependability"": 0.60134628722506867,
        ""explosionFactor"": 0.056846367221719755,
        ""id"": 1118,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1127"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""First Lion has candidiasis""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1127/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1126"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1126
    },
    ""id"": 1127,
    ""name"": ""First Lion"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1125"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1127""
        },
        ""id"": 1125,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/50""
        },
        ""sku"": ""2728"",
        ""startsOn"": ""2012-10-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1123"",
        ""dependability"": 0.0981381633775952,
        ""id"": 1123,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1124"",
        ""dependability"": 0.66451787281060493,
        ""id"": 1124,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/15""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1130"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Admired Raccoon has imperteigo""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1130/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1129"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1129
    },
    ""id"": 1130,
    ""name"": ""Admired Raccoon"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1128"",
        ""_type"": ""Gun"",
        ""dependability"": 0.16480587709918892,
        ""explosionFactor"": 0.751379841822842,
        ""id"": 1128,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/49""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1137"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Third Mountain has abscess""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1137/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1136"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1136
    },
    ""id"": 1137,
    ""name"": ""Third Mountain"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1135"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1137""
        },
        ""id"": 1135,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""sku"": ""6421"",
        ""startsOn"": ""2012-08-11T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1131"",
        ""dependability"": 0.69357266961297614,
        ""id"": 1131,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1132"",
        ""dependability"": 0.7692307642517755,
        ""id"": 1132,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/51""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1133"",
        ""dependability"": 0.84820450881878129,
        ""id"": 1133,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/23""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1134"",
        ""dependability"": 0.31153633180611595,
        ""id"": 1134,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/21""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1142"",
    ""_type"": ""MusicalCritter"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Disgusting Mustang has epilepsy""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1142/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1141"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1141
    },
    ""id"": 1142,
    ""instrument"": ""Spanish Xylophone"",
    ""name"": ""Disgusting Mustang"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1139"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1142"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1139,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/55""
        },
        ""sku"": ""242"",
        ""startsOn"": ""2012-09-20T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1140"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1142"",
          ""_type"": ""MusicalCritter""
        },
        ""id"": 1140,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/64""
        },
        ""sku"": ""7311"",
        ""startsOn"": ""2012-08-30T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1138"",
        ""_type"": ""Gun"",
        ""dependability"": 0.80145982364260582,
        ""explosionFactor"": 0.3689590554539855,
        ""id"": 1138,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/41""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1146"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Neighboring Porpoise has cataract""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1146/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1145"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1145
    },
    ""id"": 1146,
    ""name"": ""Neighboring Porpoise"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/1143"",
        ""_type"": ""Gun"",
        ""dependability"": 0.24462129373318575,
        ""explosionFactor"": 0.16549196800472771,
        ""id"": 1143,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1144"",
        ""dependability"": 0.40035272175462577,
        ""id"": 1144,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/68""
        }
      }
    ]
  },
  {
    ""_uri"": ""http://localhost:2211/critter/1154"",
    ""crazyValue"": {
      ""info"": ""Yup, this is a value object. Look.. no _ref URI."",
      ""sickness"": ""Wild Buffalo has blindness""
    },
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/1154/enemies""
    },
    ""hat"": {
      ""_uri"": ""http://localhost:2211/hat/1153"",
      ""hatType"": ""Hat#890783066"",
      ""id"": 1153
    },
    ""id"": 1154,
    ""name"": ""Wild Buffalo"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": [
      {
        ""_uri"": ""http://localhost:2211/subscription/1151"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1154""
        },
        ""id"": 1151,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/40""
        },
        ""sku"": ""372"",
        ""startsOn"": ""2012-11-21T08:17:25.8236086Z""
      },
      {
        ""_uri"": ""http://localhost:2211/subscription/1152"",
        ""critter"": {
          ""_ref"": ""http://localhost:2211/critter/1154""
        },
        ""id"": 1152,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        },
        ""sku"": ""5683"",
        ""startsOn"": ""2012-11-12T08:17:25.8236086Z""
      }
    ],
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/1147"",
        ""dependability"": 0.73761465574503626,
        ""id"": 1147,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1148"",
        ""dependability"": 0.10059414296438644,
        ""id"": 1148,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/61""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/gun/1149"",
        ""_type"": ""Gun"",
        ""dependability"": 0.40027170879778995,
        ""explosionFactor"": 0.79920329190753558,
        ""id"": 1149,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/28""
        }
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/1150"",
        ""dependability"": 0.84325384248199586,
        ""id"": 1150,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/53""
        }
      }
    ]
  }
]";

        [Test]
        public void CJson2_Test()
        {
            var encoder = new CJson2Encoder();
            var memstream = new MemoryStream();
            encoder.Stream = memstream;

            var jtoken = JToken.Parse(bigJsonFileWithCritters);
            encoder.PackIt(jtoken);
            memstream.Flush();
            memstream.Seek(0, SeekOrigin.Begin);

            var originalBytes = Encoding.UTF8.GetBytes(JToken.Parse(bigJsonFileWithCritters).ToString(Formatting.None));
            var packedBytes = memstream.ToArray();
            var originalBytesZippedLength = GetZippedLength(originalBytes);
            var packedBytesZippedLength = GetZippedLength(packedBytes);
            Console.WriteLine("Size uncompressed: {0} ({1} gz'ed)", originalBytes.Length, originalBytesZippedLength);
            Console.WriteLine("Size compressed: {0} ({1}%) ({2} ({3}%) gz'ed)", packedBytes.Length,
                              100.0*(double) packedBytes.Length/originalBytes.Length, packedBytesZippedLength,
                              100.0*(double) packedBytesZippedLength/originalBytesZippedLength);
            Console.WriteLine("Total cached property names: " + encoder.TotalCachedPropertyNames);
            Console.WriteLine("Total signatures: " + encoder.TotalCachedSignatures);
        }


        [Test]
        public void EncodeEmptyJsonArray()
        {
            Parse("[]");
        }


        [Test]
        public void EncodeEmptyJsonObject()
        {
            Parse("{}");
        }


        [Test]
        public void EncodeJsonObjectWithOneProperty_WherePropNameIsNonEscaped()
        {
            Parse(" {   luv : \"fskjkana\" }");
        }


        [Test]
        public void EncodeJsonObjectWithOneProperty_WherePropNameIsString()
        {
            Parse(" {   \"bola\": \"nananana\" }");
        }


        [Test]
        public void EncodeJsonObjectWithOneProperty_WhereValueIsNumber()
        {
            Parse(" {   luv : 343455 }");
        }


        [Test]
        public void EncodeLotsaCritters()
        {
            Parse(bigJsonFileWithCritters);
        }
    }
}