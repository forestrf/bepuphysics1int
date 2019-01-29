﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FixMath.NET
{
    public class Fix64Tests
    {
		static int OneRaw = Fix64.One.ToRaw();

        int[] m_testCases = new int[] {
            // Small numbers
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
            -1, -2, -3, -4, -5, -6, -7, -8, -9, -10,
  
            // Integer numbers
			1 * OneRaw, 1 * -OneRaw, 2 * OneRaw, 2 * -OneRaw, 3 * OneRaw, 3 * -OneRaw,
			4 * OneRaw, 4 * -OneRaw, 5 * OneRaw, 5 * -OneRaw, 6 * OneRaw, 6 * -OneRaw,
			7 * OneRaw, 7 * -OneRaw, 8 * OneRaw, 8 * -OneRaw, 9 * OneRaw, 9 * -OneRaw,
  
            // Fractions (1/2, 1/4, 1/8)
			OneRaw / 2, -OneRaw / 2, OneRaw / 4, -OneRaw / 4, OneRaw / 8, -OneRaw / 8,
  
            // Problematic carry
			OneRaw - 1, - (OneRaw - 1),
			(OneRaw - 1) * 2 + 1, - ((OneRaw - 1) * 2 + 1),
			(OneRaw - 1) * 4 + 3, - ((OneRaw - 1) * 4 + 3),
  
            // Smallest and largest values
            int.MaxValue, int.MinValue,
			
            // Smallest and largest values minus one
            int.MaxValue - 1, int.MinValue + 1,
  
            // Large random numbers
            630801836, -819412065, 622276300, -781232034,
            848005112, -767148444, 770879513, -521836768,
            223221456, -520369580, 686049339, -906349457,
            658689347, -655205245, 670526493,

            // Small random numbers
            -4368, -2246, 3294, 25981, 3398, 13, 1060,
            -34527, 1753, 28913, 24813, 2850, 265, -1447,
            30773,

            // Tiny random numbers
            -171,
            -359, 491, 844, 158, -413, -422, -737, -575, -330,
            -376, 435, -311, 116, 715, -1024, -487, 59, 724, 993
        };

        [Test]
        public void T1_IntToFix64AndBack() {
			List<int> sources = new List<int>() {
				int.MinValue,
				-1000000,
				-100000,
				-10000,
				-1000,
				-100,
				-10,
				-1,
				0,
				1,
				10,
				100,
				1000,
				10000,
				100000,
				1000000,
				int.MaxValue,
			};

			Random r = new Random(0);

			for (int i = 0; i < 100; i++) {
				sources.Add(r.Next());
			}

			foreach (var value in sources) {
				int expected = value > (int) Fix64.MaxValue ? (int) Fix64.MaxValue :
					value < (int) Fix64.MinValue ? (int) Fix64.MinValue :
					value;
				Assert.AreEqual(expected, (int) (Fix64) value, value.ToString());
			}
		}

        [Test]
        public void T2_DoubleToFix64AndBack() {
            List<double> sources = new List<double>() {
				-int.MaxValue * 100d,
				-int.MaxValue * 2d,
				-int.MaxValue,
				int.MinValue,
				-(double)Math.PI,
                -(double)Math.E,
                -1.0,
                -0.0,
                0.0,
                1.0,
                (double)Math.PI,
                (double)Math.E,
				int.MaxValue,
				int.MaxValue * 2d,
				int.MaxValue * 100d,
			};

			Random r = new Random(0);

			for (int i = 0; i < 100; i++) {
				sources.Add(r.NextDouble());
			}

            foreach (var value in sources) {
				var expected = value > (double) Fix64.MaxValue ? (double) Fix64.MaxValue :
					value < (double) Fix64.MinValue ? (double) Fix64.MinValue :
					value;
				Assert.AreEqual(expected, (double) (Fix64) value, (double) Fix64.Precision);
            }
        }
		
        [Test]
        public void T3_DecimalToFix64AndBack() {
			Assert.AreEqual(Fix64.MaxValue, (Fix64) (decimal) Fix64.MaxValue);
			Assert.AreEqual(Fix64.MinValue, (Fix64) (decimal) Fix64.MinValue);
			
			List<decimal> sources = new List<decimal>() {
				-int.MaxValue * 100m,
				-int.MaxValue * 2m,
				-int.MaxValue,
				int.MinValue,
				-(decimal)Math.PI,
				-(decimal)Math.E,
				-1.0m,
				-0.0m,
				0.0m,
				1.0m,
				(decimal)Math.PI,
				(decimal)Math.E,
				int.MaxValue,
				int.MaxValue * 2m,
				int.MaxValue * 100m,
			};

			Random r = new Random(0);

			for (int i = 0; i < 100; i++) {
				sources.Add((decimal) r.NextDouble());
			}

			foreach (var value in sources) {
				var expected = value > (decimal) Fix64.MaxValue ? (decimal) Fix64.MaxValue :
					value < (decimal) Fix64.MinValue ? (decimal) Fix64.MinValue :
					value;
				Assert.AreEqual((double) expected, (double) (decimal) (Fix64) value, (double) Fix64.Precision);
			}
		}

		[Test]
		public void T4_Addition() {
			var terms1 = new[] { Fix64.MinValue, (Fix64) (-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
			var terms2 = new[] { (Fix64) (-1), (Fix64) 2, (Fix64) (-1.5m), (Fix64) (-2), Fix64.One };
			var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64) (-1.5m), (Fix64) (-1), Fix64.MaxValue };
			for (int i = 0; i < terms1.Length; ++i) {
				var actual = terms1[i] + terms2[i];
				var expected = expecteds[i];
				Assert.AreEqual(expected, actual, terms1[i] + " + " + terms2[i]);
			}


			for (int i = 0; i < m_testCases.Length; ++i) {
				for (int j = 0; j < m_testCases.Length; ++j) {
					var x = Fix64.FromRaw(m_testCases[i]);
					var y = Fix64.FromRaw(m_testCases[j]);

					double expected = Saturate((double) x + (double) y);
					double actual = (double) (x + y);

					Assert.AreEqual(expected, actual, x.ToString() + " + " + y.ToString());
				}
			}
		}

		[Test]
		public void T5_Substraction() {
			var terms1 = new[] { Fix64.MinValue, (Fix64) (-1), Fix64.Zero, Fix64.One, Fix64.MaxValue };
			var terms2 = new[] { Fix64.One, (Fix64) (-2), (Fix64) (1.5m), (Fix64) (2), (Fix64) (-1) };
			var expecteds = new[] { Fix64.MinValue, Fix64.One, (Fix64) (-1.5m), (Fix64) (-1), Fix64.MaxValue };
			for (int i = 0; i < terms1.Length; ++i) {
				var actual = terms1[i] - terms2[i];
				var expected = expecteds[i];
				Assert.AreEqual(expected, actual, terms1[i] + " - " + terms2[i]);
			}

			for (int i = 0; i < m_testCases.Length; ++i) {
				for (int j = 0; j < m_testCases.Length; ++j) {
					var x = Fix64.FromRaw(m_testCases[i]);
					var y = Fix64.FromRaw(m_testCases[j]);

					double expected = Saturate((double) x - (double) y);
					double actual = (double) (x - y);

					Assert.AreEqual(expected, actual, x.ToString() + " - " + y.ToString());
				}
			}
		}

		[Test]
		public void T6_Negation() {
			foreach (var operand1 in m_testCases) {
				var f = Fix64.FromRaw(operand1);
				if (f == Fix64.MinValue) {
					Assert.AreEqual(-f, Fix64.MaxValue);
				}
				else {
					var expected = -((decimal) f);
					var actual = (decimal) (-f);
					Assert.AreEqual(expected, actual);
				}
			}
		}

		[Test]
		public void T7_EqualityInequalityComparisonOperators() {
			List<Fix64> sources = m_testCases.Select(Fix64.FromRaw).ToList();
			foreach (var op1 in sources) {
				foreach (var op2 in sources) {
					var d1 = (double) op1;
					var d2 = (double) op2;
					Assert.AreEqual(d1 == d2, op1 == op2);
					Assert.AreEqual(d1.Equals(d2), op1.Equals(op2));
					Assert.AreEqual(d1 != d2, op1 != op2);
					Assert.AreNotEqual(op1 != op2, op1 == op2);
					Assert.AreEqual(d1 < d2, op1 < op2);
					Assert.AreEqual(d1 <= d2, op1 <= op2);
					Assert.AreEqual(d1 > d2, op1 > op2);
					Assert.AreEqual(d1 >= d2, op1 >= op2);
				}
			}
		}

		[Test]
		public void T8_CompareTo() {
			var nums = m_testCases.Select(Fix64.FromRaw).ToArray();
			var numsDecimal = nums.Select(t => (decimal) t).ToArray();
			Array.Sort(nums);
			Array.Sort(numsDecimal);
			CollectionAssert.AreEqual(numsDecimal, nums.Select(t => (decimal) t));
		}

		[Test]
		public void T9_Sign() {
			var sources = new[] { Fix64.MinValue, (Fix64) (-128), (Fix64) (-100), (Fix64) (-1), Fix64.Zero, Fix64.One, (Fix64) 100, (Fix64) 128, Fix64.MaxValue };
			var expecteds = new int[] { -1, -1, -1, -1, 0, 1, 1, 1, 1 };
			for (int i = 0; i < sources.Length; ++i) {
				var expected = expecteds[i];

				var actual = Fix64.Sign(sources[i]);
				Assert.AreEqual((double) expected, (double) actual, sources[i].ToString());

				var actual2 = Fix64.SignI(sources[i]);
				Assert.AreEqual((double) expected, (double) actual2, sources[i].ToString());
			}
		}



		[Test]
        public void BasicMultiplication()
        {
            var term1s = new[] { 0m, 1m, -1m, 5m, -5m, 0.5m, -0.5m, -1.0m };
            var term2s = new[] { 16m, 16m, 16m, 16m, 16m, 16m, 16m, -1.0m };
            var expecteds = new[] { 0L, 16, -16, 80, -80, 8, -8, 1 };
            for (int i = 0; i < term1s.Length; ++i)
            {
                var expected = expecteds[i];
                var actual = (long)((Fix64)term1s[i] * (Fix64)term2s[i]);
               Assert.AreEqual(expected, actual, term1s[i] + " * " + term2s[i]);
            }
        }

        [Test]
        public void MultiplicationTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var x = Fix64.FromRaw(m_testCases[i]);
                    var y = Fix64.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;
                    var expected = xM * yM;
                    expected =
                        expected > (decimal)Fix64.MaxValue
                            ? (decimal)Fix64.MaxValue
                            : expected < (decimal)Fix64.MinValue
                                  ? (decimal)Fix64.MinValue
                                  : expected;
                    sw.Start();
                    var actual = x * y;
                    sw.Stop();
                    var actualM = (decimal)actual;
                    var maxDelta = (decimal)Fix64.FromRaw(1);
                    if (Math.Abs(actualM - expected) > maxDelta)
                    {
                        Console.WriteLine("Failed for FromRaw({0}) * FromRaw({1}): expected {2} but got {3}",
                                          m_testCases[i],
                                          m_testCases[j],
                                          (Fix64)expected,
                                          actualM);
                        ++failures;
						Assert.Fail("Failed for FromRaw({0}) * FromRaw({1}): expected {2} but got {3}",
										  m_testCases[i],
										  m_testCases[j],
										  (Fix64) expected,
										  actualM);
                    }
                }
            }
            Console.WriteLine("{0} total, {1} per multiplication", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.True(failures < 1);
        }
		
        [Test]
        public void DivisionTestCases()
        {
            var sw = new Stopwatch();
            int failures = 0;
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var x = Fix64.FromRaw(m_testCases[i]);
                    var y = Fix64.FromRaw(m_testCases[j]);
                    var xM = (decimal)x;
                    var yM = (decimal)y;

                    if (m_testCases[j] == 0)
                    {
						Assert.AreEqual(x >= 0 ? Fix64.MaxValue : Fix64.MinValue, x / y, x.ToString() + " / " + y.ToString());
                    }
                    else
                    {
                        var expected = xM / yM;
                        expected =
                            expected > (decimal)Fix64.MaxValue
                                ? (decimal)Fix64.MaxValue
                                : expected < (decimal)Fix64.MinValue
                                      ? (decimal)Fix64.MinValue
                                      : expected;
                        sw.Start();
                        var actual = x / y;
                        sw.Stop();
                        var actualM = (decimal)actual;
                        var maxDelta = (decimal)Fix64.FromRaw(1);
                        if (Math.Abs(actualM - expected) > maxDelta)
                        {
                            Console.WriteLine("Failed for FromRaw({0}) / FromRaw({1}): expected {2} but got {3}",
                                              m_testCases[i],
                                              m_testCases[j],
                                              (Fix64)expected,
                                              actualM);
                            ++failures;
							Assert.Fail("Failed for FromRaw({0}) / FromRaw({1}): expected {2} but got {3}",
											  m_testCases[i],
											  m_testCases[j],
											  (Fix64) expected,
											  actualM);
                        }
                    }
                }
            }
            Console.WriteLine("{0} total, {1} per division", sw.ElapsedMilliseconds, (double)sw.Elapsed.Milliseconds / (m_testCases.Length * m_testCases.Length));
            Assert.True(failures < 1);
        }
		
        [Test]
        public void Abs()
        {
           Assert.AreEqual(Fix64.MaxValue, Fix64.Abs(Fix64.MinValue));
			var sources = new[] { -(int) Fix64.MaxValue, -1, 0, 1, (int) Fix64.MaxValue - 1, (int) Fix64.MaxValue };
			var expecteds = new[] { (int) Fix64.MaxValue, 1, 0, 1, (int) Fix64.MaxValue - 1, (int) Fix64.MaxValue };
			for (int i = 0; i < sources.Length; ++i)
            {
                var actual = Fix64.Abs((Fix64)sources[i]);
                var expected = (Fix64)expecteds[i];
               Assert.AreEqual(expected, actual, sources[i].ToString());
            }
        }

        [Test]
        public void FastAbs()
        {
           Assert.AreEqual(Fix64.MinValue, Fix64.FastAbs(Fix64.MinValue));
            var sources = new[] { -(int) Fix64.MaxValue, -1, 0, 1, (int) Fix64.MaxValue - 1, (int) Fix64.MaxValue };
            var expecteds = new[] { (int) Fix64.MaxValue, 1, 0, 1, (int) Fix64.MaxValue - 1, (int) Fix64.MaxValue };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = Fix64.FastAbs((Fix64)sources[i]);
                var expected = (Fix64)expecteds[i];
               Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Floor()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -6m, -1, 0, 1, 5m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)Fix64.Floor((Fix64)sources[i]);
                var expected = expecteds[i];
               Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void Ceiling()
        {
            var sources = new[] { -5.1m, -1, 0, 1, 5.1m };
            var expecteds = new[] { -5m, -1, 0, 1, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
                var actual = (decimal)Fix64.Ceiling((Fix64)sources[i]);
                var expected = expecteds[i];
               Assert.AreEqual(expected, actual);
            }

           Assert.AreEqual(Fix64.MaxValue, Fix64.Ceiling(Fix64.MaxValue));
        }

        [Test]
        public void Round()
        {
            var sources = new[] { -5.5m, -5.1m, -4.5m, -4.4m, -1, 0, 1, 4.5m, 4.6m, 5.4m, 5.5m };
            var expecteds = new[] { -6m, -5m, -4m, -4m, -1, 0, 1, 4m, 5m, 5m, 6m };
            for (int i = 0; i < sources.Length; ++i)
            {
				var actualFix64 = Fix64.Round((Fix64) sources[i]);
				var actual = (decimal) actualFix64;
                var expected = expecteds[i];
               Assert.AreEqual(expected, actual);
            }
           Assert.AreEqual(Fix64.MaxValue, Fix64.Round(Fix64.MaxValue));
        }
		
        [Test]
        public void Sqrt()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var f = Fix64.FromRaw(m_testCases[i]);
                if (Fix64.Sign(f) < 0)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Sqrt(f));
                }
                else
                {
                    var expected = Math.Sqrt((double)f);
                    var actual = (double)Fix64.Sqrt(f);
					Assert.AreEqual(expected, actual, (double) Fix64.Precision, "sqrt(" + f + ")");
                }
            }
        }

        [Test]
        public void Log2()
        {
            double maxDelta = 4 * (double) Fix64.Precision;

            for (int j = 0; j < m_testCases.Length; ++j)
            {
                var b = Fix64.FromRaw(m_testCases[j]);

                if (b <= Fix64.Zero)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Log2(b));
                }
                else
                {
                    var expected = Math.Log((double)b) / Math.Log(2);
                    var actual = (double)Fix64.Log2(b);
                    var delta = Math.Abs(expected - actual);

					Assert.AreEqual(expected, actual, maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Test]
        public void Ln()
        {
            double maxDelta = 4 * (double) Fix64.Precision;

            for (int j = 0; j < m_testCases.Length; ++j)
            {
                var b = Fix64.FromRaw(m_testCases[j]);

                if (b <= Fix64.Zero)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Ln(b));
                }
                else
                {
                    var expected = Math.Log((double)b);
                    var actual = (double)Fix64.Ln(b);
                    var delta = Math.Abs(expected - actual);

                    Assert.AreEqual(expected, actual, maxDelta, string.Format("Ln({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
        }

        [Test]
        public void Pow2()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var e = Fix64.FromRaw(m_testCases[i]);

                var expected = Math.Min(Math.Pow(2, (double)e), (double)Fix64.MaxValue);
                var actual = (double)Fix64.Pow2(e);

				double maxDelta = Math.Abs((double) e) > 100000000 ? 0.5 :
					expected > 100000000 ? 10 :
					expected > 1000 ? 0.5 :
					32 * (double) Fix64.Precision;

				Assert.AreEqual(expected, actual, maxDelta, string.Format("Pow2({0}) = expected {1} but got {2}", e, expected, actual));
            }
        }

        [Test]
        public void Pow()
        {
            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var b = Fix64.FromRaw(m_testCases[i]);

                for (int j = 0; j < m_testCases.Length; ++j)
                {
                    var e = Fix64.FromRaw(m_testCases[j]);

                    if (b == Fix64.Zero && e < Fix64.Zero)
                    {
                        Assert.Throws<DivideByZeroException>(() => Fix64.Pow(b, e));
                    }
                    else if (b < Fix64.Zero && e != Fix64.Zero)
                    {
                        Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Pow(b, e));
                    }
                    else
                    {
                        var expected = e == Fix64.Zero ? 1 : b == Fix64.Zero ? 0 : Math.Min(Math.Pow((double)b, (double)e), (double)Fix64.MaxValue);

						// Absolute precision deteriorates with large result values, take this into account
						// Similarly, large exponents reduce precision, even if result is small.
						double maxDelta =
							Math.Abs((double) e) > 100000000 ? 0.5 :
							expected > 100000000 ? 10 :
							expected > 10000 ? 1 :
							expected > 1000 ? 0.5 :
							expected > 100 ? 0.1 :
							expected > 50 ? 0.01 :
							expected > 10 ? 0.001 :
							expected > 1 ? 0.0005 :
							16 * (double) Fix64.Precision;

						var actual = (double)Fix64.Pow(b, e);
                        var delta = Math.Abs(expected - actual);

                        Assert.AreEqual(expected, actual, maxDelta, string.Format("Pow({0}, {1}) = expected {2} but got {3}", b, e, expected, actual));
                    }
                }
            }
        }

        [Test]
        public void Modulus()
        {
            var deltas = new List<decimal>();
            foreach (var operand1 in m_testCases)
            {
                foreach (var operand2 in m_testCases)
                {
                    var f1 = Fix64.FromRaw(operand1);
                    var f2 = Fix64.FromRaw(operand2);

                    if (operand2 == 0)
                    {
						Assert.AreEqual(f1 >= 0 ? Fix64.MaxValue : Fix64.MinValue, f1 / f2);
                    }
                    else
                    {
                        var d1 = (decimal)f1;
                        var d2 = (decimal)f2;
                        var actual = (decimal)(f1 % f2);
                        var expected = d1 % d2;
                        var delta = Math.Abs(expected - actual);
                        deltas.Add(delta);
                        Assert.True(delta <= 60 * Fix64.Precision, string.Format("{0} % {1} = expected {2} but got {3}", f1, f2, expected, actual));
                    }
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("failed: {0}%", deltas.Count(d => d > Fix64.Precision) * 100.0 / deltas.Count);
        }

		/*
        [Test]
        public void SinBenchmark()
        {
            var deltas = new List<double>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            // Restricting the range to from 0 to Pi/2
            for (var angle = 0.0; angle <= 2 * Math.PI; angle += 0.000004)
            {
                var f = (Fix64)angle;
                swf.Start();
                var actualF = Fix64.Sin(f);
                swf.Stop();
                var actual = (double)actualF;
                swd.Start();
                var expectedD = Math.Sin(angle);
                swd.Stop();
                var expected = (double)expectedD;
                var delta = Math.Abs(expected - actual);
                deltas.Add(delta);
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / (double)Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / (double)Fix64.Precision);
            Console.WriteLine("Fix64.Sin time = {0}ms, Math.Sin time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }
		*/

        [Test]
        public void Sin()
        {
            Assert.AreEqual(Fix64.Zero, Fix64.Sin(Fix64.Zero), "sin(0)");

            Assert.AreEqual(Fix64.One, Fix64.Sin(Fix64.PiOver2), "sin(PI^2)");
            Assert.AreEqual(Fix64.Zero, Fix64.Sin(Fix64.Pi), "sin(PI)");
            Assert.AreEqual(-Fix64.One, Fix64.Sin(Fix64.Pi + Fix64.PiOver2), "sin(PI + PI^2)");
            Assert.AreEqual(Fix64.Zero, Fix64.Sin(Fix64.PiTimes2), "sin(2 * PI)");

            Assert.AreEqual(-Fix64.One, Fix64.Sin(-Fix64.PiOver2), "sin(-PI^2)");
            Assert.AreEqual(Fix64.Zero, Fix64.Sin(-Fix64.Pi), "sin(-PI)");
            Assert.AreEqual(Fix64.One, Fix64.Sin(-Fix64.Pi - Fix64.PiOver2), "sin(-PI - PI^2)");
            Assert.AreEqual((double) Fix64.Zero, (double) Fix64.Sin(-Fix64.PiTimes2), (double) Fix64.Precision, "sin(-2 * PI)"); // This doesn't return exactly 0


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Sin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.LessOrEqual(delta, 3 * Fix64.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            var deltas = new List<decimal>();
            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.Sin(f);
                var expected = Math.Sin((double)f);
                Assert.AreEqual(expected, (double) actualF, 16 * (double) Fix64.Precision, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Test]
        public void FastSin()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.FastSin(f);
                var expected = (decimal)Math.Sin(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * Fix64.Precision, string.Format("Sin({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.FastSin(f);
                var expected = (decimal)Math.Sin((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Sin({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Test]
        public void Acos()
        {
            var deltas = new List<decimal>();

           Assert.AreEqual(Fix64.Zero, Fix64.Acos(Fix64.One));
           Assert.AreEqual(Fix64.PiOver2, Fix64.Acos(Fix64.Zero));
           Assert.AreEqual(Fix64.Pi, Fix64.Acos(-Fix64.One));

            // Precision
            for (var x = -1.0; x < 1.0; x += 0.001)
            {
                var xf = (Fix64)x;
                var actual = (double) Fix64.Acos(xf);
                var expected = Math.Acos((double) xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add((decimal) delta);
				Assert.AreEqual(expected, actual, 12 * (double) Fix64.Precision, string.Format("Precision: Acos({0}): expected {1} but got {2}", xf, expected, actual));
            }

            for (int i = 0; i < m_testCases.Length; ++i)
            {
                var b = Fix64.FromRaw(m_testCases[i]);

                if (b < -Fix64.One || b > Fix64.One)
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => Fix64.Acos(b));
                }
                else
                {
                    var expected = Math.Acos((double) b);
                    var actual = (double) Fix64.Acos(b);
                    var delta = Math.Abs(expected - actual);
                    deltas.Add((decimal) delta);
					Assert.AreEqual(expected, actual, 16 * (double) Fix64.Precision, string.Format("Acos({0}) = expected {1} but got {2}", b, expected, actual));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }

        [Test]
        public void Cos()
        {
            Assert.True(Fix64.Cos(Fix64.Zero) == Fix64.One);

            Assert.True(Fix64.Cos(Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(Fix64.Pi) == -Fix64.One);
            Assert.True(Fix64.Cos(Fix64.Pi + Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(Fix64.PiTimes2) == Fix64.One);

            Assert.True(Fix64.Cos(-Fix64.PiOver2) == -Fix64.Zero);
            Assert.True(Fix64.Cos(-Fix64.Pi) == -Fix64.One);
            Assert.True(Fix64.Cos(-Fix64.Pi - Fix64.PiOver2) == Fix64.Zero);
            Assert.True(Fix64.Cos(-Fix64.PiTimes2) == Fix64.One);


            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Cos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
				Assert.LessOrEqual(delta, 3 * Fix64.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.Cos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
				Assert.LessOrEqual(delta, 16 * Fix64.Precision, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Test]
        public void FastCos()
        {
            for (double angle = -2 * Math.PI; angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.FastCos(f);
                var expected = (decimal)Math.Cos(angle);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 50000 * Fix64.Precision, string.Format("Cos({0}): expected {1} but got {2}", angle, expected, actualF));
            }

            foreach (var val in m_testCases)
            {
                var f = Fix64.FromRaw(val);
                var actualF = Fix64.FastCos(f);
                var expected = (decimal)Math.Cos((double)f);
                var delta = Math.Abs(expected - (decimal)actualF);
                Assert.True(delta <= 0.01M, string.Format("Cos({0}): expected {1} but got {2}", f, expected, actualF));
            }
        }

        [Test]
        public void Tan()
        {
            Assert.True(Fix64.Tan(Fix64.Zero) == Fix64.Zero);
            Assert.True(Fix64.Tan(Fix64.Pi) == Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.Pi) == Fix64.Zero);

            Assert.True(Fix64.Tan(Fix64.PiOver2 - (Fix64)0.001) > Fix64.Zero);
            Assert.True(Fix64.Tan(Fix64.PiOver2 + (Fix64)0.001) < Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.PiOver2 - (Fix64)0.001) > Fix64.Zero);
            Assert.True(Fix64.Tan(-Fix64.PiOver2 + (Fix64)0.001) < Fix64.Zero);

            for (double angle = 0;/*-2 * Math.PI;*/ angle <= 2 * Math.PI; angle += 0.0001)
            {
                var f = (Fix64)angle;
                var actualF = Fix64.Tan(f);
                var expected = (decimal)Math.Tan(angle);
               Assert.AreEqual(actualF > Fix64.Zero, expected > 0);
                //TODO figure out a real way to test this function
            }

            //foreach (var val in m_testCases) {
            //    var f = (Fix64)val;
            //    var actualF = Fix64.Tan(f);
            //    var expected = (decimal)Math.Tan((double)f);
            //    var delta = Math.Abs(expected - (decimal)actualF);
            //    Assert.True(delta <= 0.01, string.Format("Tan({0}): expected {1} but got {2}", f, expected, actualF));
            //}
        }

        [Test]
        public void Atan()
        {
            var deltas = new List<decimal>();

           Assert.AreEqual(Fix64.Zero, Fix64.Atan(Fix64.Zero));

            // Precision
            for (var x = -1.0; x < 1.0; x += 0.0001)
            {
                var xf = (Fix64)x;
                var actual = (decimal)Fix64.Atan(xf);
                var expected = (decimal)Math.Atan((double)xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add(delta);
                Assert.LessOrEqual(delta, 10 * Fix64.Precision, string.Format("Precision: Atan({0}): expected {1} but got {2}", xf, expected, actual));
            }

            // Scalability and edge cases
            foreach (var x in m_testCases)
            {
                var xf = Fix64.FromRaw(x);
                var actual = (decimal)Fix64.Atan(xf);
                var expected = (decimal)Math.Atan((double)xf);
                var delta = Math.Abs(actual - expected);
                deltas.Add(delta);
                Assert.LessOrEqual(delta, 10 * Fix64.Precision, string.Format("Scalability: Atan({0}): expected {1} but got {2}", xf, expected, actual));
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }

		/*
        [Test]
        public void AtanBenchmark()
        {
            var deltas = new List<decimal>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            for (var x = -1.0; x < 1.0; x += 0.001)
            {
                for (int k = 0; k < 1000; ++k)
                {
                    var xf = (Fix64)x;
                    swf.Start();
                    var actualF = Fix64.Atan(xf);
                    swf.Stop();
                    swd.Start();
                    var expected = Math.Atan((double)xf);
                    swd.Stop();
                    deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("Fix64.Atan time = {0}ms, Math.Atan time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }
		*/

        [Test]
        public void Atan2()
        {
            var deltas = new List<decimal>();
            // Identities
           Assert.AreEqual(Fix64.Atan2(Fix64.Zero, -Fix64.One), Fix64.Pi);
           Assert.AreEqual(Fix64.Atan2(Fix64.Zero, Fix64.Zero), Fix64.Zero);
           Assert.AreEqual(Fix64.Atan2(Fix64.Zero, Fix64.One), Fix64.Zero);
           Assert.AreEqual(Fix64.Atan2(Fix64.One, Fix64.Zero), Fix64.PiOver2);
           Assert.AreEqual(Fix64.Atan2(-Fix64.One, Fix64.Zero), -Fix64.PiOver2);

            // Precision
            for (var y = -1.0; y < 1.0; y += 0.01)
            {
                for (var x = -1.0; x < 1.0; x += 0.01)
                {
                    var yf = (Fix64)y;
                    var xf = (Fix64)x;
                    var actual = (double) Fix64.Atan2(yf, xf);
                    var expected = (double) Math.Atan2((double)yf, (double)xf);
                    var delta = Math.Abs(actual - expected);
					deltas.Add((decimal) delta);
					Assert.AreEqual(expected, actual, (double) 0.005, string.Format("Precision: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }

            // Scalability and edge cases
            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    var yf = Fix64.FromRaw(y);
                    var xf = Fix64.FromRaw(x);
                    var actual = (double) Fix64.Atan2(yf, xf);
                    var expected = (double) Math.Atan2((double) yf, (double) xf);
                    var delta = Math.Abs(actual - expected);
                    deltas.Add((decimal) delta);
                    Assert.AreEqual(expected, actual, (double) 0.005, string.Format("Scalability: Atan2({0}, {1}): expected {2} but got {3}", yf, xf, expected, actual));
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
        }

		/*
        [Test]
        public void Atan2Benchmark()
        {
            var deltas = new List<decimal>();

            var swf = new Stopwatch();
            var swd = new Stopwatch();

            foreach (var y in m_testCases)
            {
                foreach (var x in m_testCases)
                {
                    for (int k = 0; k < 1000; ++k)
                    {
                        var yf = (Fix64)y;
                        var xf = (Fix64)x;
                        swf.Start();
                        var actualF = Fix64.Atan2(yf, xf);
                        swf.Stop();
                        swd.Start();
                        var expected = Math.Atan2((double)yf, (double)xf);
                        swd.Stop();
                        deltas.Add(Math.Abs((decimal)actualF - (decimal)expected));
                    }
                }
            }
            Console.WriteLine("Max error: {0} ({1} times precision)", deltas.Max(), deltas.Max() / Fix64.Precision);
            Console.WriteLine("Average precision: {0} ({1} times precision)", deltas.Average(), deltas.Average() / Fix64.Precision);
            Console.WriteLine("Fix64.Atan2 time = {0}ms, Math.Atan2 time = {1}ms", swf.ElapsedMilliseconds, swd.ElapsedMilliseconds);
        }
		*/

#if UNITY_EDITOR // Only override Console inside Unity
		public static class Console {
			public static void WriteLine(string text) {
				UnityEngine.Debug.Log(text);
			}
			public static void WriteLine(string text, params object[] args) {
				UnityEngine.Debug.LogFormat(text, args);
			}
		}
#endif

		static double Saturate(double v) {
			return Math.Max((double) Fix64.MinValue, Math.Min((double) Fix64.MaxValue, v));
		}
	}
}
