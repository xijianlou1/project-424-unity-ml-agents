﻿using System;

namespace Perrinn424.PerformanceBenchmarkSystem
{
    public static class PerformanceBenchmarkHelper
    {
        public static int[] porsche919Data = new[]
           {
             0,41,93,146,186,217,244,278,324,382,448,518,586,653,724,797,874,937,988,1032,1078,1132,1191,1242,1287,1328,1375,1428,1473,1512,1552,1602,1663,1733,1810,1890,1973,2057,2143,2222,2300,2375,2453,2532,2613,2697,2783,2872,2963,3056,3151,3245,3335,3423,3507,3588,3671,3753,3810,3854,3895,3935,3986,4049,4120,4199,4280,4365,4450,4538,4627,4718,4810,4891,4961,5031,5088,5143,5184,5215,5245,5279,5313,5359,5416,5484,5559,5637,5718,5802,5885,5955,6025,6098,6154,6198,6242,6295,6353,6414,6456,6491,6530,6580,6638,6701,6766,6831,6902,6964,7025,7088,7155,7227,7291,7341,7379,7406,7430,7463,7508,7565,7627,7695,7767,7835,7882,7923,7972,8028,8079,8122,8167,8221,8285,8357,8436,8518,8602,8688,8772,8835,8883,8923,8966,9019,9080,9150,9226,9306,9388,9471,9554,9638,9722,9808,9896,9984,10074,10169,10265,10362,10457,10550,10643,10734,10808,10879,10953,11028,11106,11185,11260,11337,11415,11482,11530,11564,11595,11630,11674,11727,11784,11847,11906,11948,11976,12002,12029,12055,12084,12120,12169,12228,12294,12364,12436,12510,12586,12663,12728,12787,12845,12902,12957,13005,13052,13108,13169,13236,13306,13376,13432,13489,13546,13605,13669,13722,13767,13817,13874,13929,13978,14028,14082,14133,14179,14226,14283,14341,14385,14426,14472,14527,14585,14631,14670,14711,14762,14821,14885,14955,15027,15103,15171,15233,15295,15358,15421,15486,15556,15629,15703,15781,15861,15942,16027,16113,16200,16287,16375,16444,16512,16577,16627,16674,16727,16786,16838,16875,16908,16942,16987,17043,17105,17171,17229,17285,17346,17410,17473,17540,17613,17693,17779,17869,17962,18059,18159,18261,18363,18464,18566,18668,18770,18872,18973,19074,19175,19276,19377,19477,19576,19674,19771,19868,19965,20058,20146,20231,20314,20378,20429,20467,20498,20532,20570,20611,20639,20669,20702
           };

        public static int[] IDRData = new int[]
            {
              0,39,86,139,184,218,247,277,314,361,415,474,537,600,662,727,796,868,930,981,1023,1063,1104,1153,1206,1254,1295,1332,1368,1412,1462,1506,1543,1576,1619,1667,1723,1784,1850,1919,1991,2064,2138,2211,2283,2352,2416,2485,2556,2628,2702,2775,2849,2923,2998,3073,3149,3223,3297,3370,3443,3517,3586,3655,3727,3795,3844,3883,3917,3954,3998,4048,4105,4168,4237,4310,4384,4459,4534,4610,4686,4760,4831,4899,4963,5024,5077,5121,5166,5204,5237,5266,5295,5323,5358,5402,5455,5514,5578,5645,5717,5790,5864,5935,5997,6058,6123,6176,6217,6253,6295,6343,6398,6449,6487,6519,6554,6598,6646,6705,6769,6830,6897,6965,7023,7077,7134,7197,7265,7323,7369,7404,7431,7455,7484,7522,7568,7621,7681,7745,7813,7871,7916,7955,7995,8042,8090,8126,8162,8207,8259,8317,8382,8451,8521,8593,8666,8740,8807,8860,8902,8937,8976,9022,9071,9126,9186,9250,9317,9385,9455,9524,9594,9664,9735,9806,9878,9947,10017,10087,10157,10226,10294,10362,10431,10500,10568,10637,10705,10772,10836,10901,10965,11030,11096,11162,11229,11297,11365,11431,11491,11534,11565,11592,11622,11661,11706,11758,11815,11876,11924,11958,11985,12009,12033,12056,12082,12115,12158,12210,12267,12330,12395,12461,12527,12593,12659,12721,12776,12828,12877,12928,12976,13019,13061,13112,13171,13235,13301,13364,13422,13475,13530,13582,13637,13695,13742,13787,13838,13896,13948,13996,14045,14099,14149,14191,14238,14292,14347,14387,14422,14461,14509,14563,14615,14654,14691,14731,14778,14832,14894,14961,15029,15098,15168,15226,15277,15332,15385,15443,15508,15573,15640,15710,15780,15847,15913,15981,16051,16119,16189,16259,16329,16398,16461,16519,16577,16623,16667,16714,16767,16825,16867,16899,16931,16972,17019,17075,17135,17196,17250,17304,17361,17423,17488,17551,17613,17677,17741,17807,17873,17941,18010,18079,18149,18218,18288,18357,18426,18496,18565,18634,18703,18772,18841,18910,18978,19046,19113,19181,19249,19316,19384,19451,19519,19587,19655,19723,19792,19862,19931,20001,20070,20137,20203,20268,20333,20391,20439,20474,20503,20534,20572,20614,20646,20675,20705
            };

        public static PerformanceBenchmark CreatePorsche919()
        {
            var porsche = new PerformanceBenchmark(Array.ConvertAll(porsche919Data, Convert.ToSingle), frequency : 1f);

            return porsche;
        }

        public static PerformanceBenchmark CreateIDR()
        {
            var idr = new PerformanceBenchmark(Array.ConvertAll(IDRData, Convert.ToSingle), frequency: 1);


            return idr;
        }
    }
}
