﻿namespace Engine.Services
{
    public class PerlinNoiseService
    {
        private static readonly int[] P; // Doubled permutation to avoid overflow

        private static readonly int[] Permutation =
        {
            151,
            160,
            137,
            91,
            90,
            15, // Hash lookup table as defined by Ken Perlin.  This is a randomly
            131,
            13,
            201,
            95,
            96,
            53,
            194,
            233,
            7,
            225,
            140,
            36,
            103,
            30,
            69,
            142,
            8,
            99,
            37,
            240,
            21,
            10,
            23, // arranged array of all numbers from 0-255 inclusive.
            190,
            6,
            148,
            247,
            120,
            234,
            75,
            0,
            26,
            197,
            62,
            94,
            252,
            219,
            203,
            117,
            35,
            11,
            32,
            57,
            177,
            33,
            88,
            237,
            149,
            56,
            87,
            174,
            20,
            125,
            136,
            171,
            168,
            68,
            175,
            74,
            165,
            71,
            134,
            139,
            48,
            27,
            166,
            77,
            146,
            158,
            231,
            83,
            111,
            229,
            122,
            60,
            211,
            133,
            230,
            220,
            105,
            92,
            41,
            55,
            46,
            245,
            40,
            244,
            102,
            143,
            54,
            65,
            25,
            63,
            161,
            1,
            216,
            80,
            73,
            209,
            76,
            132,
            187,
            208,
            89,
            18,
            169,
            200,
            196,
            135,
            130,
            116,
            188,
            159,
            86,
            164,
            100,
            109,
            198,
            173,
            186,
            3,
            64,
            52,
            217,
            226,
            250,
            124,
            123,
            5,
            202,
            38,
            147,
            118,
            126,
            255,
            82,
            85,
            212,
            207,
            206,
            59,
            227,
            47,
            16,
            58,
            17,
            182,
            189,
            28,
            42,
            223,
            183,
            170,
            213,
            119,
            248,
            152,
            2,
            44,
            154,
            163,
            70,
            221,
            153,
            101,
            155,
            167,
            43,
            172,
            9,
            129,
            22,
            39,
            253,
            19,
            98,
            108,
            110,
            79,
            113,
            224,
            232,
            178,
            185,
            112,
            104,
            218,
            246,
            97,
            228,
            251,
            34,
            242,
            193,
            238,
            210,
            144,
            12,
            191,
            179,
            162,
            241,
            81,
            51,
            145,
            235,
            249,
            14,
            239,
            107,
            49,
            192,
            214,
            31,
            181,
            199,
            106,
            157,
            184,
            84,
            204,
            176,
            115,
            121,
            50,
            45,
            127,
            4,
            150,
            254,
            138,
            236,
            205,
            93,
            222,
            114,
            67,
            29,
            24,
            72,
            243,
            141,
            128,
            195,
            78,
            66,
            215,
            61,
            156,
            180
        };
        // MODIFIED IMPLEMENTATION OF IMPROVED NOISE (COPYRIGHT 2002 KEN PERLIN) FROM 3D to 2D WITH FURTHER ADDITIONS FROM  Flafla2.

        private readonly int repeat;

        static PerlinNoiseService()
        {
            P = new int[512];
            for (var x = 0; x < 512; x++)
            {
                P[x] = Permutation[x % 256];
            }
        }

        public PerlinNoiseService(int repeat = -1)
        {
            this.repeat = repeat;
        }

        public double OctavePerlin(
            double x,
            double y,
            int octaves,
            double persistence)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0; // Used for normalizing result to 0.0 - 1.0
            for (var i = 0; i < octaves; i++)
            {
                total += Perlin(x * frequency, y * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        public double Perlin(double x, double y)
        {
            double z = 1;
            if (repeat > 0)
            {
                // If we have any repeat on, change the coordinates to their "local" repetitions
                x = x % repeat;
                y = y % repeat;
                z = z % repeat;
            }

            var xi = (int) x & 255; // Calculate the "unit cube" that the point asked will be located in
            var yi = (int) y & 255; // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
            var zi = (int) z & 255; // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.

            var xf = x - (int) x; // We also fade the location to smooth the result.
            var yf = y - (int) y;
            var zf = z - (int) z;
            var u = Fade(xf);
            var v = Fade(yf);
            var w = Fade(zf);

            var aaa = P[P[P[xi] + yi] + zi];
            var aba = P[P[P[xi] + Inc(yi)] + zi];
            var aab = P[P[P[xi] + yi] + Inc(zi)];
            var abb = P[P[P[xi] + Inc(yi)] + Inc(zi)];
            var baa = P[P[P[Inc(xi)] + yi] + zi];
            var bba = P[P[P[Inc(xi)] + Inc(yi)] + zi];
            var bab = P[P[P[Inc(xi)] + yi] + Inc(zi)];
            var bbb = P[P[P[Inc(xi)] + Inc(yi)] + Inc(zi)];

            var x1 = Lerp(
                Grad(aaa, xf, yf, zf), // The gradient function calculates the dot product between a pseudorandom
                Grad(baa, xf - 1, yf, zf), // gradient vector and the vector from the input coordinate to the 8
                u);
            var x2 = Lerp(
                Grad(aba, xf, yf - 1, zf), // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                Grad(bba, xf - 1, yf - 1, zf), // values we made earlier.
                u);
            var y1 = Lerp(x1, x2, v);

            x1 = Lerp(Grad(aab, xf, yf, zf - 1), Grad(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Grad(abb, xf, yf - 1, zf - 1), Grad(bbb, xf - 1, yf - 1, zf - 1), u);
            var y2 = Lerp(x1, x2, v);

            return (Lerp(y1, y2, w) + 1) / 2; // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
        }

        private int Inc(int num)
        {
            num++;
            if (repeat > 0)
            {
                num %= repeat;
            }

            return num;
        }

        private static double Grad(
            int hash,
            double x,
            double y,
            double z)
        {
            var h = hash & 15; // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
            var u = h < 8 /* 0b1000 */
                ? x
                : y; // If the most significant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

            double v; // In Ken Perlin's original implementation this was another conditional operator (?:).  I
            // expanded it for readability.

            if (h < 4 /* 0b0100 */) // If the first and second significant bits are 0 set v = y
            {
                v = y;
            }
            else if (h == 12 /* 0b1100 */ ||
                h == 14 /* 0b1110*/
            ) // If the first and second significant bits are 1 set v = x
            {
                v = x;
            }
            else // If the first and second significant bits are not equal (0/1, 1/0) set v = z
            {
                v = z;
            }

            return
                ((h & 1) == 0 ? u : -u) +
                ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
        }

        private static double Fade(double t) =>
            // Fade function as defined by Ken Perlin.  This eases coordinate values
            // so that they will "ease" towards integral values.  This ends up smoothing
            // the final output.
            t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3

        private static double Lerp(double a, double b, double x) => a + x * (b - a);
    }
}