namespace MotorClaims.Models
{
    public static class HelpersDate
    {
        public static string FormatDate(DateTime oDate)
        {
            return oDate.ToString("dd-MM-yyyy h:mm tt");
        }

        public static string FormatLongDate(DateTime oDate)
        {
            return oDate.ToString("ddd, dd-MM-yyyy h:mm tt");
        }

        public static string FormatLongDateNoTime(DateTime oDate)
        {
            return oDate.ToString("ddddd, dd-MM-yyyy");
        }

        public static string FormatDateNoTime(DateTime oDate)
        {
            return oDate.ToString("dd-MM-yyyy");
        }

        public static string FormatShortDateNoTime(DateTime oDate)
        {
            return oDate.ToString("ddd, dd-MM-yyyy");
        }


        public static string GetShortDayName(DateTime oDate)
        {
            return oDate.ToString("ddd");
        }

        public static string GetDayName(DateTime oDate)
        {
            return oDate.ToString("ddddd");
        }

        public static string getShortMonthName(DateTime oDate)
        {
            return oDate.ToString("MMM");
        }
    }
}
