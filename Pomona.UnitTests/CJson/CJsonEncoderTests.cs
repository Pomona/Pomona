#region License

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

#endregion

using System;

using NUnit.Framework;

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


        private string bigJsonFileWithCritters =
            @"[
  {
    ""_uri"": ""http://localhost:2211/critter/15"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/15/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/14""
    },
    ""name"": ""Woeful Bighorn"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/15/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/12"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.36341230075965275,
        ""dependability"": 0.24978887813621614,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 12
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/13"",
        ""dependability"": 0.1267881594257374,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""id"": 13
      }
    ],
    ""id"": 15
  },
  {
    ""_uri"": ""http://localhost:2211/critter/21"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/21/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/20""
    },
    ""name"": ""Svelte Hedgehog"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/21/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/16"",
        ""dependability"": 0.8547456235879779,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""id"": 16
      }
    ],
    ""id"": 21
  },
  {
    ""_uri"": ""http://localhost:2211/critter/24"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/24/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/23""
    },
    ""name"": ""Cooperative Waterbuck"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/24/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/22"",
        ""dependability"": 0.589860097314166,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 22
      }
    ],
    ""id"": 24
  },
  {
    ""_uri"": ""http://localhost:2211/critter/30"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/30/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/29""
    },
    ""name"": ""Authorized Bison"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/30/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/25"",
        ""dependability"": 0.80990086906119285,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""id"": 25
      },
      {
        ""_uri"": ""http://localhost:2211/gun/26"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.71768433587517788,
        ""dependability"": 0.30890309871588978,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""id"": 26
      },
      {
        ""_uri"": ""http://localhost:2211/gun/27"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.27259672073302638,
        ""dependability"": 0.87499871471663881,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""id"": 27
      }
    ],
    ""id"": 30
  },
  {
    ""_uri"": ""http://localhost:2211/critter/38"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/38/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/37""
    },
    ""name"": ""Rough Buffalo"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/38/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/31"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.23618424834505852,
        ""dependability"": 0.95922974821144236,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""id"": 31
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/32"",
        ""dependability"": 0.41914385530126458,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""id"": 32
      },
      {
        ""_uri"": ""http://localhost:2211/gun/33"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.54130237109088453,
        ""dependability"": 0.18643747418487328,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""id"": 33
      }
    ],
    ""id"": 38
  },
  {
    ""_uri"": ""http://localhost:2211/critter/46"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/46/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/45""
    },
    ""name"": ""Adorable Ocelot"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/46/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/39"",
        ""dependability"": 0.070791292037252013,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""id"": 39
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/40"",
        ""dependability"": 0.72256009593725212,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 40
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/41"",
        ""dependability"": 0.88442391663995756,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""id"": 41
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/42"",
        ""dependability"": 0.807800937354472,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 42
      }
    ],
    ""id"": 46
  },
  {
    ""_uri"": ""http://localhost:2211/critter/51"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/51/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/50""
    },
    ""name"": ""Surprised Leopard"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/51/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/47"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.68203188836669171,
        ""dependability"": 0.99927987158264964,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""id"": 47
      }
    ],
    ""id"": 51
  },
  {
    ""_uri"": ""http://localhost:2211/critter/60"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/60/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/59""
    },
    ""name"": ""Modern Alpaca"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/60/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/52"",
        ""dependability"": 0.72696263283815354,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""id"": 52
      },
      {
        ""_uri"": ""http://localhost:2211/gun/53"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.95870454374640457,
        ""dependability"": 0.97362302940973222,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""id"": 53
      },
      {
        ""_uri"": ""http://localhost:2211/gun/54"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.1575479843456056,
        ""dependability"": 0.48002275520936716,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 54
      },
      {
        ""_uri"": ""http://localhost:2211/gun/55"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.23880144732017136,
        ""dependability"": 0.030237304526491697,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""id"": 55
      }
    ],
    ""id"": 60
  },
  {
    ""_uri"": ""http://localhost:2211/critter/65"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/65/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/64""
    },
    ""name"": ""Hoarse Whale"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/65/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/61"",
        ""dependability"": 0.57638444545510437,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""id"": 61
      },
      {
        ""_uri"": ""http://localhost:2211/gun/62"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.0678904303665694,
        ""dependability"": 0.11714311927423958,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""id"": 62
      }
    ],
    ""id"": 65
  },
  {
    ""_uri"": ""http://localhost:2211/critter/71"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/71/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/70""
    },
    ""name"": ""Exemplary Sheep"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/71/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/66"",
        ""dependability"": 0.52448390402108613,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""id"": 66
      },
      {
        ""_uri"": ""http://localhost:2211/gun/67"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.1615267215117471,
        ""dependability"": 0.1464834451426209,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""id"": 67
      }
    ],
    ""id"": 71
  },
  {
    ""_uri"": ""http://localhost:2211/critter/78"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/78/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/77""
    },
    ""name"": ""Steep Budgerigar"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/78/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/72"",
        ""dependability"": 0.47460879593836552,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""id"": 72
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/73"",
        ""dependability"": 0.90363830975426285,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 73
      },
      {
        ""_uri"": ""http://localhost:2211/gun/74"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.96234687415992226,
        ""dependability"": 0.241719328910913,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/5""
        },
        ""id"": 74
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/75"",
        ""dependability"": 0.80737121999607009,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 75
      }
    ],
    ""id"": 78
  },
  {
    ""_uri"": ""http://localhost:2211/critter/85"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/85/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/84""
    },
    ""name"": ""Concerned Rat"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/85/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/79"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.32256902629629197,
        ""dependability"": 0.24447514686941874,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""id"": 79
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/80"",
        ""dependability"": 0.82849534313590045,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/3""
        },
        ""id"": 80
      }
    ],
    ""id"": 85
  },
  {
    ""_uri"": ""http://localhost:2211/critter/90"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/90/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/89""
    },
    ""name"": ""Major Guanaco"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/90/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/86"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.18413657098269862,
        ""dependability"": 0.24819008505353243,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""id"": 86
      },
      {
        ""_uri"": ""http://localhost:2211/gun/87"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.41416111374933323,
        ""dependability"": 0.78194290761926344,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""id"": 87
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/88"",
        ""dependability"": 0.95433537892733489,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 88
      }
    ],
    ""id"": 90
  },
  {
    ""_uri"": ""http://localhost:2211/critter/99"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/99/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/98""
    },
    ""name"": ""Humming Wolf"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/99/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/91"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.59928859472241658,
        ""dependability"": 0.15203976964207355,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 91
      },
      {
        ""_uri"": ""http://localhost:2211/gun/92"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.6250650974107278,
        ""dependability"": 0.70092665297022394,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 92
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/93"",
        ""dependability"": 0.73424023051477982,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/4""
        },
        ""id"": 93
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/94"",
        ""dependability"": 0.60915789688432487,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/2""
        },
        ""id"": 94
      }
    ],
    ""id"": 99
  },
  {
    ""_uri"": ""http://localhost:2211/critter/106"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/106/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/105""
    },
    ""name"": ""Dark Finch"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/106/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/100"",
        ""dependability"": 0.94316449246516665,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""id"": 100
      },
      {
        ""_uri"": ""http://localhost:2211/gun/101"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.36773217905672834,
        ""dependability"": 0.059624527608800923,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/11""
        },
        ""id"": 101
      }
    ],
    ""id"": 106
  },
  {
    ""_uri"": ""http://localhost:2211/critter/110"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/110/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/109""
    },
    ""name"": ""Spotless Kid"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/110/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/107"",
        ""dependability"": 0.36659028211915412,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/9""
        },
        ""id"": 107
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/108"",
        ""dependability"": 0.44245698416720003,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/1""
        },
        ""id"": 108
      }
    ],
    ""id"": 110
  },
  {
    ""_uri"": ""http://localhost:2211/critter/117"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/117/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/116""
    },
    ""name"": ""Shallow Chameleon"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/117/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/111"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.66080873723179512,
        ""dependability"": 0.43966451400875323,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""id"": 111
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/112"",
        ""dependability"": 0.40105496505324495,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""id"": 112
      }
    ],
    ""id"": 117
  },
  {
    ""_uri"": ""http://localhost:2211/critter/122"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/122/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/121""
    },
    ""name"": ""Fond Addax"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/122/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/118"",
        ""dependability"": 0.52191813454121261,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/8""
        },
        ""id"": 118
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/119"",
        ""dependability"": 0.47639832947235478,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/10""
        },
        ""id"": 119
      }
    ],
    ""id"": 122
  },
  {
    ""_uri"": ""http://localhost:2211/critter/126"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/126/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/125""
    },
    ""name"": ""Dreary Alligator"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/126/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/weapon/123"",
        ""dependability"": 0.51483012852949561,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 123
      }
    ],
    ""id"": 126
  },
  {
    ""_uri"": ""http://localhost:2211/critter/132"",
    ""createdOn"": ""0001-01-01T00:00:00"",
    ""enemies"": {
      ""_ref"": ""http://localhost:2211/critter/132/enemies""
    },
    ""hat"": {
      ""_ref"": ""http://localhost:2211/hat/131""
    },
    ""name"": ""Live Kitten"",
    ""okdayIsFun"": ""jada"",
    ""subscriptions"": {
      ""_ref"": ""http://localhost:2211/critter/132/subscriptions""
    },
    ""weapons"": [
      {
        ""_uri"": ""http://localhost:2211/gun/127"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.598667684755599,
        ""dependability"": 0.9614948397323,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/7""
        },
        ""id"": 127
      },
      {
        ""_uri"": ""http://localhost:2211/gun/128"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.45992373743090953,
        ""dependability"": 0.42937211386364516,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""id"": 128
      },
      {
        ""_uri"": ""http://localhost:2211/weapon/129"",
        ""dependability"": 0.426653894794478,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/0""
        },
        ""id"": 129
      },
      {
        ""_uri"": ""http://localhost:2211/gun/130"",
        ""_type"": ""Gun"",
        ""explosionFactor"": 0.5244898332862602,
        ""dependability"": 0.76428889239406628,
        ""model"": {
          ""_ref"": ""http://localhost:2211/weaponmodel/6""
        },
        ""id"": 130
      }
    ],
    ""id"": 132
  }
]";


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
            Parse(this.bigJsonFileWithCritters);
        }
    }
}