﻿//#define USE_DOUBLES // Used for testing
//#define CHECK_OVERFLOW
// + ".\n" + new System.Diagnostics.StackTrace(true)

using FixPointCS;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a Q1.15.16 fixed-point number.
/// </summary>
public enum Fix32 : int {
	MaxValue = int.MaxValue,
	MinValue = int.MinValue,
	

	Six = One * 6,
	Five = One * 5,
	Four = One * 4,
	Three = One * 3,
	Two = One * 2,
	One = Fix32Ext.ONE,
	Half = One / 2,
	Third = One / 3,
	Fourth = One / 4,
	Zero = 0,
	MinusOne = -One,
	MinusTwo = -One * 2,
	MinusThree = -One * 3,

	Pi = Fix32Ext.PI,
	PiOver2 = Fix32Ext.PI_OVER_2,
	PiOver4 = Fix32Ext.PI_OVER_4,
	PiTimes2 = Fix32Ext.PI_TIMES_2,
	PiInv = Fix32Ext.PI_INV,
	PiOver2Inv = Fix32Ext.PI_OVER_2_INV,
	E = Fix32Ext.E_RAW,
	EPow4 = Fix32Ext.EPOW4,
	Ln2 = Fix32Ext.LN2,
	Log2Max = Fix32Ext.LOG2MAX,
	Log2Min = Fix32Ext.LOG2MIN,

	Precision = 1,
}

/// <summary>
/// Fix32 number stored in a long.
/// Used inside operations when more precision is needed for the integer side.
/// </summary>
public enum Fix32L : long {
	MaxValue = long.MaxValue,
	MinValue = long.MinValue,
}

/// <summary>
/// Operations for <see cref="Fix32"/>.
/// </summary>
public static partial class Fix32Ext {

	// Precision of this type is 2^-14, that is 6.103515625E-5
	public static readonly double Precision = ((Fix32) 1).ToDouble();

	public const int MAX_INT_VALUE = int.MaxValue >> FRACTIONAL_BITS;
	public const int MIN_INT_VALUE = int.MinValue >> FRACTIONAL_BITS;
	public const int NUM_BITS = 32;
	public const int FRACTIONAL_BITS = 16;
	public const int SIGNED_INTEGER_BITS = NUM_BITS - FRACTIONAL_BITS;

	public const int NUM_BITS_MINUS_ONE = NUM_BITS - 1;
	public const int ONE = 1 << FRACTIONAL_BITS;
	public const int HALF = ONE >> 1;
	public const int FRACTIONAL_MASK = ONE - 1;
	public const int INTEGER_MASK = ~FRACTIONAL_MASK;
	public const int SIGN_MASK = unchecked((int) (1U << NUM_BITS_MINUS_ONE));
	public const int LOG2MAX = (NUM_BITS - 1 - FRACTIONAL_BITS) << FRACTIONAL_BITS;
	public const int LOG2MIN = -((NUM_BITS - FRACTIONAL_BITS) << FRACTIONAL_BITS);
	internal const int LUT_SIZE_RS = FRACTIONAL_BITS / 2 - 1;
	internal const int LUT_SIZE = PI_OVER_2 >> LUT_SIZE_RS;

	static readonly Fix32 LutInterval = ((LUT_SIZE - 1) / PI_OVER_2).ToFix();
	static readonly Fix32 C0p28 = 0.28.ToFix();

	// Const before rounding
	const decimal D_PI = ONE * (3.1415926535897932384626433832795028841971693993751058209749445923078164m);
	const decimal D_PI_TIMES_2 = ONE * (2m * 3.1415926535897932384626433832795028841971693993751058209749445923078164m);
	const decimal D_PI_OVER_2 = ONE * (3.1415926535897932384626433832795028841971693993751058209749445923078164m / 2m);
	const decimal D_PI_OVER_4 = ONE * (3.1415926535897932384626433832795028841971693993751058209749445923078164m / 4m);
	const decimal D_PI_INV = ONE * (1m / 3.1415926535897932384626433832795028841971693993751058209749445923078164m);
	const decimal D_PI_OVER_2_INV = ONE * (1m / (3.1415926535897932384626433832795028841971693993751058209749445923078164m / 2m));
	const decimal D_E_RAW = ONE * (2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200m);
	const decimal D_EPOW4 = ONE * (2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200m * 2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200m * 2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200m * 2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200m);
	const decimal D_LN2 = ONE * (0.693147180559945309417232121458m);

	// Const rounded to int instead of truncated
	internal const int PI = (int) (D_PI < 0 ? D_PI - 0.5m : D_PI + 0.5m);
	internal const int PI_TIMES_2 = (int) (D_PI_TIMES_2 < 0 ? D_PI_TIMES_2 - 0.5m : D_PI_TIMES_2 + 0.5m);
	internal const int PI_OVER_2 = (int) (D_PI_OVER_2 < 0 ? D_PI_OVER_2 - 0.5m : D_PI_OVER_2 + 0.5m);
	internal const int PI_OVER_4 = (int) (D_PI_OVER_4 < 0 ? D_PI_OVER_4 - 0.5m : D_PI_OVER_4 + 0.5m);
	internal const int PI_INV = (int) (D_PI_INV < 0 ? D_PI_INV - 0.5m : D_PI_INV + 0.5m);
	internal const int PI_OVER_2_INV = (int) (D_PI_OVER_2_INV < 0 ? D_PI_OVER_2_INV - 0.5m : D_PI_OVER_2_INV + 0.5m);
	internal const int E_RAW = (int) (D_E_RAW < 0 ? D_E_RAW - 0.5m : D_E_RAW + 0.5m);
	internal const int EPOW4 = (int) (D_EPOW4 < 0 ? D_EPOW4 - 0.5m : D_EPOW4 + 0.5m);
	internal const int LN2 = (int) (D_LN2 < 0 ? D_LN2 - 0.5m : D_LN2 + 0.5m);



	/// <summary>
	/// Adds x and y. Overflows.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Add(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() + y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if ((long) x + (long) y < int.MinValue || (long) x + (long) y > int.MaxValue) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " + " + y.ToStringExt() + " = " + (x.ToDouble() + y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		return AddSafe(x, y);
		return (Fix32) ((int) x + (int) y);
	}

	/// <summary>
	/// Adds x and y. Saturates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 AddSafe(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() + y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if ((long) x + (long) y < int.MinValue || (long) x + (long) y > int.MaxValue) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " + " + y.ToStringExt() + " = " + (x.ToDouble() + y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		// https://stackoverflow.com/questions/17580118/signed-saturated-add-of-64-bit-ints/17587197#17587197
		// determine the lower or upper bound of the result
		//int ret = (x.RawValue < 0) ? MIN_VALUE : MAX_VALUE;
		int ret = (int) ((((uint) x >> NUM_BITS_MINUS_ONE) - 1U) ^ (1U << NUM_BITS_MINUS_ONE));
		// this is always well defined:
		// if x < 0 this adds a positive value to INT64_MIN
		// if x > 0 this subtracts a positive value from INT64_MAX
		//int comp = ret - xRaw;
		// the condition is equivalent to
		// ((x < 0) && (y > comp)) || ((x >=0) && (y <= comp))
		return (Fix32) ((x < 0) != ((int) y > (ret - (int) x)) ? ret : (int) x + (int) y);
	}

	/// <summary>
	/// Subtracts y from x. Overflows.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Sub(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() - y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if ((long) x - (long) y < int.MinValue || (long) x - (long) y > int.MaxValue) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " - " + y.ToStringExt() + " = " + (x.ToDouble() - y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		return SubSafe(x, y);
		return (Fix32) ((int) x - (int) y);
	}

	/// <summary>
	/// Subtracts y from x. Saturates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 SubSafe(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() - y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if ((long) x - (long) y < int.MinValue || (long) x - (long) y > int.MaxValue) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " - " + y.ToStringExt() + " = " + (x.ToDouble() - y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		long sub = (long) x - (long) y; // TO TEST: Shift and operate to check overflow
		return (Fix32) (((int) sub) != sub ? (int) ((((uint) x >> NUM_BITS_MINUS_ONE) - 1U) ^ (1U << NUM_BITS_MINUS_ONE)) : (int) sub);
	}

	/// <summary>
	/// Negate
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Neg(this Fix32 x) {
#if USE_DOUBLES
		return (-x.ToDouble()).ToFix();
#endif
		//return new Fix32(-x.RawValue);
		return x == Fix32.MinValue ? Fix32.MaxValue : (Fix32) (-(int) x);
	}

	/// <summary>
	/// Returns a number indicating the sign of a Fix32 number.
	/// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SignI(this Fix32 x) {
		//https://stackoverflow.com/questions/14579920/fast-sign-of-integer-in-c/14612418#14612418
		return ((int) x >> NUM_BITS_MINUS_ONE) | (int) (((uint) -(int) x) >> NUM_BITS_MINUS_ONE);
	}

	/// <summary>
	/// Returns a number indicating the sign of a Fix32 number.
	/// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Sign(this Fix32 x) {
		const int RS = NUM_BITS_MINUS_ONE - FRACTIONAL_BITS;
		return (Fix32) ((((int) x >> RS) | (int) (((uint) -(int) x) >> RS)) & INTEGER_MASK);
	}

	/// <summary>
	/// Returns the absolute value of a Fix32 number.
	/// Note: Abs(Fix32.MinValue) == Fix32.MaxValue.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Abs(this Fix32 x) {
#if USE_DOUBLES
		return Math.Abs(x.ToDouble()).ToFix();
#endif
		if (x == Fix32.MinValue) {
			return Fix32.MaxValue;
		}

		// branchless implementation, see http://www.strchr.com/optimized_abs_function
		var mask = (int) x >> NUM_BITS_MINUS_ONE;
		return (Fix32) (((int) x + mask) ^ mask);
	}

	/// <summary>
	/// Returns the absolute value of a Fix32 number.
	/// FastAbs(Fix32.MinValue) is undefined.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 AbsFast(this Fix32 x) {
		var mask = (int) x >> NUM_BITS_MINUS_ONE;
		return (Fix32) (((int) x + mask) ^ mask);
	}

	/// <summary>
	/// Returns the largest integer less than or equal to the specified number.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Floor(this Fix32 x) {
#if USE_DOUBLES
		return Math.Floor(x.ToDouble()).ToFix();
#endif
		// Just zero out the fractional part
		return (Fix32) ((int) x & INTEGER_MASK);
	}

	/// <summary>
	/// Returns the smallest integral value that is greater than or equal to the specified number.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Ceiling(this Fix32 x) {
#if USE_DOUBLES
		return Math.Floor(x.ToDouble()).ToFix();
#endif
		var hasFractionalPart = ((int) x & FRACTIONAL_MASK) != 0;
		return hasFractionalPart ? ((Fix32) ((int) x & INTEGER_MASK)).Add(Fix32.One) : x;
	}

	/// <summary>
	/// Rounds a value to the nearest integral value.
	/// If the value is halfway between an even and an uneven value, returns the even value.
	/// </summary>
	public static Fix32 Round(this Fix32 x) {
#if USE_DOUBLES
		return Math.Round(x.ToDouble()).ToFix();
#endif
		var fractionalPart = (int) x & FRACTIONAL_MASK;
		var integralPart = (Fix32) ((int) x & INTEGER_MASK);
		if (fractionalPart < (ONE >> 1)) {
			return integralPart;
		}
		if (fractionalPart > (ONE >> 1)) {
			return integralPart.Add(Fix32.One);
		}
		// if number is halfway between two values, round to the nearest even number
		// this is the method used by System.Math.Round().
		return ((int) integralPart & ONE) == 0 ?
			integralPart :
			integralPart.Add(Fix32.One);
	}

	/// <summary>
	/// Rounds a value to the nearest integral value.
	/// If the value is halfway between an even and an uneven value, returns the even value.
	/// FastRount(Fix32.MaxValue) is undefined
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RoundFast(this Fix32 x) {
		// https://sestevenson.wordpress.com/2009/08/19/rounding-in-fixed-point-number-conversions/
		int odd = ((int) x & ONE) >> FRACTIONAL_BITS;
		return (Fix32) (((int) x + (ONE / 2 - 1) + odd) & INTEGER_MASK);
	}

	/// <summary>
	/// Rounds a value to the nearest integral value. Overflows in the extremes.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RoundFastOverflow(this Fix32 x) {
		return (Fix32) (((int) x + HALF) & INTEGER_MASK);
	}

	/// <summary>
	/// Multiply. No saturation (overflows)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Mul(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() * y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if (x.ToDouble() * y.ToDouble() < Fix32.MinValue.ToDouble() || x.ToDouble() * y.ToDouble() > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " * " + y.ToStringExt() + " = " + (x.ToDouble() * y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		return MulSafe(x, y);
		return (Fix32) (((long) x * (long) y) >> FRACTIONAL_BITS);
	}

	/// <summary>
	/// Multiply. Saturates when overflows.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 MulSafe(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() * y.ToDouble()).ToFix();
#endif
		long multLong = ((long) x * (long) y) >> FRACTIONAL_BITS;

		int finalSign = (int) x ^ (int) y;

		return (Fix32) (((finalSign ^ multLong) & SIGN_MASK) != 0 && multLong != 0 ?
			(int) ((((uint) finalSign >> NUM_BITS_MINUS_ONE) - 1U) ^ (1U << NUM_BITS_MINUS_ONE)) :
			(int) multLong);
	}

	/// <summary>
	/// Divide. Saturates on overflow. May change to Fast.
	/// </summary>
	public static Fix32 Div(this Fix32 x, Fix32 y) {
		return DivSafe(x, y);
	}

	/// <summary>
	/// Divide. Overflows.
	/// </summary>
	public static Fix32 DivFast(this Fix32 x, Fix32 y) {
		if ((int) y == 0) {
			return (Fix32) (unchecked((int) (((((uint) x) >> NUM_BITS_MINUS_ONE) - 1U) ^ (1U << NUM_BITS_MINUS_ONE))));
			return x >= 0 ? Fix32.MaxValue : Fix32.MinValue; // Branched version of the previous code, for clarity. Slower
		}

		return (Fix32) (int) (((long) x << FRACTIONAL_BITS) / (int) y);
	}

	/// <summary>
	/// Divide. Overflows. throws when <see cref="y"/> equals 0 instead of saturating.
	/// </summary>
	public static Fix32 DivFastest(this Fix32 x, Fix32 y) {
		return (Fix32) (int) (((long) x << FRACTIONAL_BITS) / (int) y);
	}

	/// <summary>
	/// Divide. Saturates on overflow.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 DivSafe(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() / y.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		if (x.ToDouble() / y.ToDouble() < Fix32.MinValue.ToDouble() || x.ToDouble() / y.ToDouble() > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " / " + y.ToStringExt() + " = " + (x.ToDouble() / y.ToDouble()));
			//System.Diagnostics.Debugger.Break();
		}
#endif
		if ((int) y == 0) {
			return (Fix32) (unchecked((int) (((((uint) x) >> NUM_BITS_MINUS_ONE) - 1U) ^ (1U << NUM_BITS_MINUS_ONE))));
			return x >= 0 ? Fix32.MaxValue : Fix32.MinValue; // Branched version of the previous code, for clarity. Slower
		}

		long r = ((long) x << FRACTIONAL_BITS) / (int) y;
		if (r > (int) Fix32.MaxValue) return Fix32.MaxValue;
		if (r < (int) Fix32.MinValue) return Fix32.MinValue;
		return (Fix32) (int) r;
	}

	/// <summary>
	/// Returns the base-2 logarithm of a specified number.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The argument was non-positive
	/// </exception>
	public static Fix32 Log2(this Fix32 x) {
#if USE_DOUBLES
		return Math.Log(x.ToDouble(), 2).ToFix();
#endif
		if ((int) x <= 0)
			throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");

		//return (Fix32) Fixed32.Log2((int) x);

		// This implementation is based on Clay. S. Turner's fast binary logarithm
		// algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
		//     Processing Mag., pp. 124,140, Sep. 2010.)

		//https://github.com/dmoulding/log2fix/blob/master/log2fix.c

		const int EXTRA_SHIFT = 8;
		long xx = (long) x << EXTRA_SHIFT;
		const int PRECISION = FRACTIONAL_BITS + EXTRA_SHIFT;

		long b = 1 << (PRECISION - 1);
		long y = 0;

		long rawX = (long) xx;
		while (rawX < 1 << PRECISION) {
			rawX <<= 1;
			y -= 1 << PRECISION;
		}

		while (rawX >= 2 << PRECISION) {
			rawX >>= 1;
			y += 1 << PRECISION;
		}

		ulong z = (ulong) rawX;

		for (int i = 0; i < PRECISION; i++) {
			z = z * z >> PRECISION;
			if (z >= 2 << PRECISION) {
				z >>= 1;
				y += b;
			}
			b >>= 1;
		}

		return (Fix32) (int) (y >> EXTRA_SHIFT);
	}

	/// <summary>
	/// Returns the base-2 logarithm of a specified number.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The argument was non-positive
	/// </exception>
	public static Fix32 Log2Fast(this Fix32 x) {
#if USE_DOUBLES
		return Math.Log(x.ToDouble(), 2).ToFix();
#endif
		if ((int) x <= 0)
			throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");

		//return (Fix32) Fixed32.Log2((int) x);

		// This implementation is based on Clay. S. Turner's fast binary logarithm
		// algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
		//     Processing Mag., pp. 124,140, Sep. 2010.)

		//https://github.com/dmoulding/log2fix/blob/master/log2fix.c

		const int PRECISION = FRACTIONAL_BITS;

		int b = 1 << (PRECISION - 1);
		int y = 0;

		int rawX = (int) x;
		while (rawX < 1 << PRECISION) {
			rawX <<= 1;
			y -= 1 << PRECISION;
		}

		while (rawX >= 2 << PRECISION) {
			rawX >>= 1;
			y += 1 << PRECISION;
		}

		ulong z = (ulong) rawX;

		for (int i = 0; i < PRECISION; i++) {
			z = z * z >> PRECISION;
			if (z >= 2 << PRECISION) {
				z >>= 1;
				y += b;
			}
			b >>= 1;
		}

		return (Fix32) (int) (y);
	}

	/// <summary>
	/// Returns the natural logarithm of a specified number.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The argument was non-positive
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Ln(this Fix32 x) {
#if USE_DOUBLES
		return Math.Log(x.ToDouble()).ToFix();
#endif
		return Log2(x).Mul(Fix32.Ln2);
	}

	/// <summary>
	/// Returns the square root of a specified number.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The argument was negative.
	/// </exception>
	public static Fix32 SqrtSlow(this Fix32 x) {
#if USE_DOUBLES
		return Math.Sqrt(x.ToDouble()).ToFix();
#endif
		if (x <= 0) return 0;

		// https://stackoverflow.com/questions/1100090/looking-for-an-efficient-integer-square-root-algorithm-for-arm-thumb2

		// Manually calculated constants (somehow) for precision Q18.14
		const int ITERATIONS = 25;// (((NUM_BITS - 16) / 4) * 2) + 16; // 24

		int shift = NUM_BITS - 2;

		// [1] First iteration
		int bottomHalf = 0; // 0x3 & ((int) x >> SHIFT); equals 0

		int a = 0; // accumulator
		int r = 0; // remainder
		for (int i = 0; i < ITERATIONS; i++) {
			r = (r << 2) | bottomHalf;
			a <<= 1;
			int e = (a << 1) | 1; // trial product
			if (r >= e) {
				r -= e;
				a |= 1;
			}
			// [1] Subsequent iterations
			bottomHalf = 0x3 & ((int) x >> shift);
			shift -= 2;
		}

		return (Fix32) a;
	}

	/// <summary>
	/// Returns the square root of a specified number.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The argument was negative.
	/// </exception>
	public static Fix32 Sqrt(this Fix32 x) {
#if USE_DOUBLES
		return Math.Sqrt(x.ToDouble()).ToFix();
#endif

		return (Fix32) Fixed32.Sqrt((int) x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Mod(this Fix32 x, Fix32 y) {
#if USE_DOUBLES
		return (x.ToDouble() % y.ToDouble()).ToFix();
#endif
		return (Fix32) (
			x == Fix32.MinValue & (int) y == -1 ?
			0 :
			(int) x % (int) y);
	}

	/// <summary>
	/// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
	/// Use the operator (%) for a more reliable but slower modulo.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ModFast(this Fix32 x, Fix32 y) {
		return (Fix32) ((int) x % (int) y);
	}


	/// <summary>
	/// Returns 2 raised to the specified power.
	/// </summary>
	public static Fix32 Pow2(this Fix32 xx) {
#if USE_DOUBLES
		return Math.Pow(2, xx.ToDouble()).ToFix();
#endif
		Fix32 x = xx;
		if ((int) x == 0) return Fix32.One;

		// Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
		bool neg = (int) x < 0;
		if (neg) x = x.Neg();

		if ((int) x == (int) Fix32.One)
			return neg ? Fix32.One.Div(Fix32.Two) : Fix32.Two; // Can be cached
		if ((int) x >= (int) Fix32.Log2Max) return neg ? Fix32.One.Div(Fix32.MaxValue) : Fix32.MaxValue; // Can be cached
		if ((int) x <= (int) Fix32.Log2Min) return neg ? Fix32.MaxValue : Fix32.Zero;

		/*
		The algorithm is based on the power series for exp(x):
        http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
        
        From term n, we get term n+1 by multiplying with x/n.
        When the sum term drops to zero, we can stop summing.
        */
		int integerPart = x.Floor().ToInt();
		// Take fractional part of exponent
		x = (Fix32) ((uint) x & FRACTIONAL_MASK);

		Fix32 result = Fix32.One;
		Fix32 term = Fix32.One;
		Fix32 i = Fix32.One;
		while ((int) term != 0) {
			term = x.Mul(term).Mul(Fix32.Ln2.Div(i));
			result = result.Add(term);
			i = i.Add(Fix32.One);
		}

		result = (Fix32) ((int) result << integerPart);
		if (neg) result = Fix32.One.Div(result);

		return result;
	}

	/// <summary>
	/// Returns 2 raised to the specified power.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Pow2Fast(this Fix32 xx) {
#if USE_DOUBLES
		return Math.Pow(2, xx.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.Pow(2, (int) xx);
	}

	/// <summary>
	/// Returns a specified number raised to the specified power. Saturates
	/// </summary>
	/// <exception cref="DivideByZeroException">
	/// The base was zero, with a negative exponent
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// The base was negative, with a non-zero exponent
	/// </exception>
	public static Fix32 Pow(this Fix32 b, Fix32 exp) {
#if USE_DOUBLES
		return Math.Pow(b.ToDouble(), exp.ToDouble()).ToFix();
#endif
		if ((int) b == (int) Fix32.One)
			return Fix32.One;
		if ((int) exp == 0)
			return Fix32.One;
		if ((int) b == 0) {
			if ((int) exp < 0) {
				throw new DivideByZeroException();
			}
			return Fix32.Zero;
		}

		if (b < 0 && exp != 0)
			throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");

		return exp.Mul(b.Log2()).Pow2();
	}

	/// <summary>
	/// Returns the Sine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Sin(this Fix32 x) {
#if USE_DOUBLES
        return (Fix32) Math.Sin((double) x);
#endif
		return (Fix32) Fixed32.Sin((int) x);
	}

	/// <summary>
	/// Returns the Sine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 SinFast(this Fix32 x) {
		return (Fix32) Fixed32.SinFast((int) x);
	}

	/// <summary>
	/// Returns the Sine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 SinFastest(this Fix32 x) {
		return (Fix32) Fixed32.SinFastest((int) x);
	}

	/// <summary>
	/// Returns the cosine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Cos(this Fix32 x) {
#if USE_DOUBLES
		return Math.Cos(x.ToDouble()).ToFix();
#endif
		// Don't use Fixed32.Cos, it gives an error
		var rawAngle = (int) x + (x > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
		return ((Fix32) rawAngle).Sin();
	}

	/// <summary>
	/// Returns the cosine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 CosFast(this Fix32 x) {
		var rawAngle = (int) x + (x > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
		return ((Fix32) rawAngle).SinFast();
	}

	/// <summary>
	/// Returns the cosine of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 CosFastest(this Fix32 x) {
		var rawAngle = (int) x + (x > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
		return ((Fix32) rawAngle).SinFastest();
	}

	/// <summary>
	/// Returns the tangent of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Tan(this Fix32 x) {
#if USE_DOUBLES
		return Math.Tan(x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.Tan((int) x);
	}

	/// <summary>
	/// Returns the tangent of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 TanFast(this Fix32 x) {
#if USE_DOUBLES
		return Math.Tan(x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.TanFast((int) x);
	}

	/// <summary>
	/// Returns the tangent of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 TanFastest(this Fix32 x) {
#if USE_DOUBLES
		return Math.Tan(x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.TanFastest((int) x);
	}

	/// <summary>
	/// Returns the tangent of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Exp(this Fix32 x) {
#if USE_DOUBLES
		return Math.Exp(x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.Exp((int) x);
	}

	/// <summary>
	/// Returns the tangent of x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ExpFast(this Fix32 x) {
		return (Fix32) Fixed32.ExpFast((int) x);
	}

	/// <summary>
	/// Returns 1/x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Rcp(this Fix32 x) {
#if USE_DOUBLES
		return (1 / x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.Rcp((int) x);
	}

	/// <summary>
	/// Returns 1/x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RcpFast(this Fix32 x) {
#if USE_DOUBLES
		return (1 / x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.RcpFast((int) x);
	}

	/// <summary>
	/// Returns 1/x.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RcpFastest(this Fix32 x) {
#if USE_DOUBLES
		return (1 / x.ToDouble()).ToFix();
#endif
		return (Fix32) Fixed32.RcpFastest((int) x);
	}

	/// <summary>
	/// Returns 1/sqrt(x).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RSqrt(this Fix32 x) {
#if USE_DOUBLES
		return (1 / Math.Sqrt(x.ToDouble())).ToFix();
#endif
		return (Fix32) Fixed32.RSqrt((int) x);
	}

	/// <summary>
	/// Returns 1/sqrt(x).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RSqrtFast(this Fix32 x) {
#if USE_DOUBLES
		return (1 / Math.Sqrt(x.ToDouble())).ToFix();
#endif
		return (Fix32) Fixed32.RSqrtFast((int) x);
	}

	/// <summary>
	/// Returns 1/sqrt(x).
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 RSqrtFastest(this Fix32 x) {
#if USE_DOUBLES
		return (1 / Math.Sqrt(x.ToDouble())).ToFix();
#endif
		return (Fix32) Fixed32.RSqrtFastest((int) x);
	}


	/// <summary>
	/// Returns the arccos of of the specified number, calculated using Atan and Sqrt
	/// </summary>
	public static Fix32 Acos(this Fix32 x) {
#if USE_DOUBLES
        return (Fix32) Math.Acos((double) x);
#endif
		if ((int) x < -(int) Fix32.One || (int) x > (int) Fix32.One)
			throw new ArgumentOutOfRangeException(nameof(x));

		if ((int) x == 0)
			return Fix32.PiOver2;

		var result = (Fix32.One.Sub(x.Mul(x))).Sqrt().Div(x).Atan();
		return (int) x < 0 ? result.Add(Fix32.Pi) : result;
	}


	/// <summary>
	/// Returns the arctan of of the specified number, calculated using Euler series
	/// </summary>
	public static Fix32 Atan(this Fix32 zz) {
#if USE_DOUBLES
		return Math.Atan(zz.ToDouble()).ToFix();
#endif
		Fix32 z = zz;
		if ((int) z == 0)
			return Fix32.Zero;

		// Force positive values for argument
		// Atan(-z) = -Atan(z).
		bool neg = ((int) z < 0);
		if (neg) z = z.Neg();

		Fix32 result;

		if ((int) z == (int) Fix32.One)
			result = Fix32.PiOver4;
		else {
			bool invert = (int) z > (int) Fix32.One;
			if (invert) z = Fix32.One.Div(z);

			result = Fix32.One;
			Fix32 term = Fix32.One;

			Fix32 zSq = z.Mul(z);
			Fix32 zSq2 = zSq.Mul(Fix32.Two);
			Fix32 zSqPlusOne = zSq.Add(Fix32.One);
			Fix32 zSq12 = zSqPlusOne.Mul(Fix32.Two);
			Fix32 dividend = zSq2;
			Fix32 divisor = zSqPlusOne.Mul(Fix32.Three);

			for (int i = 2; i < 30; i++) {
				term = term.Mul(dividend.Div(divisor));
				result = result.Add(term);

				dividend = dividend.Add(zSq2);
				divisor = divisor.Add(zSq12);

				if ((int) term == 0)
					break;
			}

			result = result.Mul(z).Div(zSqPlusOne);

			if (invert)
				result = Fix32.PiOver2.Sub(result);
		}

		if (neg) result = result.Neg();
		return result;
	}

	public static Fix32 Atan2(this Fix32 y, Fix32 x) {
#if USE_DOUBLES
		return Math.Atan2(y.ToDouble(), x.ToDouble()).ToFix();
#endif
		var yl = (int) y;
		var xl = (int) x;
		if (xl == 0) {
			if (yl > 0)
				return Fix32.PiOver2;
			if (yl == 0)
				return Fix32.Zero;
			return Fix32.PiOver2.Neg();
		}
		Fix32 atan;
		var z = y.Div(x);

		// Deal with overflow
		if ((int) Fix32.One.Add(C0p28.Mul(z).Mul(z)) == (int) Fix32.MaxValue) {
			return (int) y < 0 ? Fix32.PiOver2.Neg() : Fix32.PiOver2;
		}

		if ((int) Abs(z) < (int) Fix32.One) {
			atan = z.Div(Fix32.One.Add(C0p28.Mul(z).Mul(z)));
			if (xl < 0) {
				if (yl < 0) {
					return atan.Sub(Fix32.Pi);
				}
				return atan.Add(Fix32.Pi);
			}
		}
		else {
			atan = Fix32.PiOver2.Sub(z.Div(z.Mul(z).Add(C0p28)));
			if (yl < 0)
				return atan.Sub(Fix32.Pi);
		}

		return atan;
	}

	public static Fix32 Atan2Fast(this Fix32 y, Fix32 x) {
#if USE_DOUBLES
		return Math.Atan2(y.ToDouble(), x.ToDouble()).ToFix();
#endif
		var yl = (int) y;
		var xl = (int) x;
		if (xl == 0) {
			if (yl > 0) {
				return Fix32.PiOver2;
			}
			if (yl == 0) {
				return Fix32.Zero;
			}
			return Fix32.PiOver2.Neg();
		}
		Fix32 atan;
		var z = y.Div(x);

		// Deal with overflow
		if ((int) Fix32.One.Add(C0p28.Mul(z).Mul(z)) == (int) Fix32.MaxValue) {
			return (int) y < 0 ? Fix32.PiOver2.Neg() : Fix32.PiOver2;
		}

		if ((int) Abs(z) < (int) Fix32.One) {
			atan = z.Div(Fix32.One.Add(C0p28.Mul(z).Mul(z)));
			if (xl < 0) {
				if (yl < 0) {
					return atan.Sub(Fix32.Pi);
				}
				return atan.Add(Fix32.Pi);
			}
		}
		else {
			atan = Fix32.PiOver2.Sub(z.Div(z.Mul(z).Add(C0p28)));
			if (yl < 0) {
				return atan.Sub(Fix32.Pi);
			}
		}
		return atan;
	}

	#region ToFrom Fix32
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFix(this float value) {
		return (Fix32) (int) Clamp(value * ONE, (int) Fix32.MinValue, (int) Fix32.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFixFast(this float value) {
		return (Fix32) (int) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ToFloat(this Fix32 value) {
		return (float) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFix(this double value) {
		return (Fix32) (int) Clamp(value * ONE, (int) Fix32.MinValue, (int) Fix32.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFixFast(this double value) {
		return (Fix32) (int) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ToDouble(this Fix32 value) {
		return (double) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFix(this int value) {
		return (Fix32) (value > MAX_INT_VALUE ? (int) Fix32.MaxValue : value < MIN_INT_VALUE ? (int) Fix32.MinValue : value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFixFast(this int value) {
		return (Fix32) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ToInt(this Fix32 value) {
		return (int) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFix(this long value) {
		return (Fix32) (value > MAX_INT_VALUE ? (int) Fix32.MaxValue : value < MIN_INT_VALUE ? (int) Fix32.MinValue : value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFixFast(this long value) {
		return (Fix32) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ToLong(this Fix32 value) {
		return (long) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFix(this decimal value) {
		return (Fix32) (int) Clamp(value * ONE, (int) Fix32.MinValue, (int) Fix32.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 ToFixFast(this decimal value) {
		return (Fix32) (int) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal ToDecimal(this Fix32 value) {
		return (decimal) value / ONE;
	}
	#endregion

	#region ToFrom Fix32L
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixL(this float value) {
		return (Fix32L) (long) Clamp(value * ONE, (long) Fix32L.MinValue, (long) Fix32L.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixLFast(this float value) {
		return (Fix32L) (long) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float ToFloat(this Fix32L value) {
		return (float) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixL(this double value) {
		return (Fix32L) (long) Clamp(value * ONE, (long) Fix32L.MinValue, (long) Fix32L.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixLFast(this double value) {
		return (Fix32L) (long) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ToDouble(this Fix32L value) {
		return (double) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixL(this int value) {
		return (Fix32L) (value > MAX_INT_VALUE ? (long) Fix32L.MaxValue : value < MIN_INT_VALUE ? (long) Fix32L.MinValue : value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixLFast(this int value) {
		return (Fix32L) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ToInt(this Fix32L value) {
		long v = (long) value / ONE;
		return v > MAX_INT_VALUE ? MAX_INT_VALUE : v < MIN_INT_VALUE ? MIN_INT_VALUE : (int) v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixL(this long value) {
		return (Fix32L) (value > MAX_INT_VALUE ? (long) Fix32L.MaxValue : value < MIN_INT_VALUE ? (long) Fix32L.MinValue : value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixLFast(this long value) {
		return (Fix32L) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ToLong(this Fix32L value) {
		return (long) value / ONE;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixL(this decimal value) {
		return (Fix32L) (long) Clamp(value * ONE, (long) Fix32L.MinValue, (long) Fix32L.MaxValue);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32L ToFixLFast(this decimal value) {
		return (Fix32L) (long) (value * ONE);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal ToDecimal(this Fix32L value) {
		return (decimal) value / ONE;
	}
	#endregion

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Clamp(float value, double min, double max) {
		return value > max ? max : value < min ? min : value;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Clamp(double value, double min, double max) {
		return value > max ? max : value < min ? min : value;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal Clamp(decimal value, decimal min, decimal max) {
		return value > max ? max : value < min ? min : value;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CompareTo(this Fix32 x, Fix32 other) {
		return ((int) x).CompareTo((int) other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToStringExt(this Fix32 x) {
		return x.ToDouble().ToString("0.##########");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToStringExt(this Fix32L x) {
		return x.ToDouble().ToString("0.##########");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 Saturate(long value) {
		return (Fix32) (int) (((((ulong) value) >> 63) - 1U) ^ (1U << NUM_BITS_MINUS_ONE));
	}

	[Conditional("CHECK_OVERFLOW")]
	private static void ReportOverflowDebug(string txt) {
		Console.WriteLine(txt);
	}

	#region Compound operators
	/// <summary>
	/// x*y - z*w
	/// No saturation (overflows)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 MulSubMul(Fix32 x, Fix32 y, Fix32 z, Fix32 w) {
#if USE_DOUBLES
		return (x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		var test = x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble();
		if (test < Fix32.MinValue.ToDouble() || test > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " * " + y.ToStringExt() + " - " + z.ToStringExt() + " * " + w.ToStringExt() + " = " + test);
			//System.Diagnostics.Debugger.Break();
		}
#endif
		return (Fix32) ((((long) x * (long) y) - ((long) z * (long) w)) >> FRACTIONAL_BITS);
	}

	/// <summary>
	/// x*y - z*w
	/// Saturates.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 MulSubMulSafe(Fix32 x, Fix32 y, Fix32 z, Fix32 w) {
#if USE_DOUBLES
		return (x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		var test = x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble();
		if (test < Fix32.MinValue.ToDouble() || test > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow " + x.ToStringExt() + " * " + y.ToStringExt() + " - " + z.ToStringExt() + " * " + w.ToStringExt() + " = " + test);
			//System.Diagnostics.Debugger.Break();
		}
#endif
		long r = (((long) x * (long) y) - ((long) z * (long) w)) >> FRACTIONAL_BITS;
		if (r > MAX_INT_VALUE) return Fix32.MaxValue;
		if (r < MIN_INT_VALUE) return Fix32.MinValue;
		return (Fix32) r;
	}

	/// <summary>
	/// (x*y - z*w) * m
	/// No saturation (overflows)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 MulSubMul_Mul(Fix32 x, Fix32 y, Fix32 z, Fix32 w, Fix32 m) {
#if USE_DOUBLES
		return ((x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()) * m.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		var test = (x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()) * m.ToDouble();
		if (test < Fix32.MinValue.ToDouble() || test > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow (" + x.ToStringExt() + " * " + y.ToStringExt() + " - " + z.ToStringExt() + " * " + w.ToStringExt() + ") * " + m.ToStringExt() + " = " + test);
			//System.Diagnostics.Debugger.Break();
		}
#endif
		long r = ((((long) x * (long) y) - ((long) z * (long) w)) * (long) m) >> (2 * FRACTIONAL_BITS);
		return (Fix32) r;
	}

	/// <summary>
	/// (x*y - z*w) * m
	/// No saturation (overflows)
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Fix32 MulSubMul_MulSafe(Fix32 x, Fix32 y, Fix32 z, Fix32 w, Fix32 m) {
#if USE_DOUBLES
		return ((x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()) * m.ToDouble()).ToFix();
#endif
#if CHECK_OVERFLOW
		var test = (x.ToDouble() * y.ToDouble() - z.ToDouble() * w.ToDouble()) * m.ToDouble();
		if (test < Fix32.MinValue.ToDouble() || test > Fix32.MaxValue.ToDouble()) {
			ReportOverflowDebug("Overflow (" + x.ToStringExt() + " * " + y.ToStringExt() + " - " + z.ToStringExt() + " * " + w.ToStringExt() + ") * " + m.ToStringExt() + " = " + test);
			//System.Diagnostics.Debugger.Break();
		}
#endif
		long r = ((((long) x * (long) y) - ((long) z * (long) w)) * (long) m) >> (2 * FRACTIONAL_BITS);
		if (r > MAX_INT_VALUE) return Fix32.MaxValue;
		if (r < MIN_INT_VALUE) return Fix32.MinValue;
		return (Fix32) r;
	}
	#endregion
}
