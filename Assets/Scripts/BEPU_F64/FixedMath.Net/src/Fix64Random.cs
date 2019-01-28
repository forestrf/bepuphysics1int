﻿using System;

namespace FixMath.NET
{
	public class Fix64Random
    {
        private Random random;

        public Fix64Random(int seed)
        {
            random = new Random(seed);
        }

        public Fix64 Next()
        {
			return Fix64.FromRaw(random.Next(int.MinValue, int.MaxValue));
        }

        public Fix64 NextInt(int maxValue)
        {
            return random.Next(maxValue);
        }
    }
}
