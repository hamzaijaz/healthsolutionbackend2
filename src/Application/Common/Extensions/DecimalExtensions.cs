using System;

namespace MyHealthSolution.Service.Application.Common.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// RoundToMoney: casts decimal value to money value
        /// </summary>
        /// <param name="value"></param>
        /// <returns> money value with 2 decimal digit precision to represent monies</returns>
        public static decimal RoundToMoney(this decimal value)
        {
            // currently rounding up all values, based on findings from E2E
            //https://stackoverflow.com/questions/7075201/rounding-up-to-2-decimal-places-in-c-sharp
            var multr = (decimal)Math.Pow(10, Convert.ToDouble(2));
            return Math.Ceiling(value * multr) / multr;
        }

        /// <summary>
        /// return if the dvalue is multiple of inputs
        /// e.g. var res = value1.MultipleOf(value2); if res is true then value1 is multiple of value2
        /// </summary>
        /// <param name="dvalue"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static bool MultipleOf(this decimal dvalue, decimal inputs)
        {
            bool isMultiple = false;

            if (inputs == 0) return isMultiple;

            var remainder = dvalue % inputs;

            if (remainder == Decimal.Zero)
            {
                isMultiple = true;
            }

            return isMultiple;
        }
    }
}