using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace Intune.Android
{
    public class DatePickerFragment : DialogFragment,
                                      DatePickerDialog.IOnDateSetListener
    {
        public static readonly string TAG = "X:" + typeof(DatePickerFragment).Name.ToUpper();
        Action<DateTime> dateSelectedHandler = delegate { };
        DateTime currenDate;

        public static DatePickerFragment NewInstance(DateTime dateSelected,
                                            Action<DateTime> onDateSelected)
        {
            DatePickerFragment dateFragment = new DatePickerFragment();
            dateFragment.dateSelectedHandler = onDateSelected;
            dateFragment.currenDate = dateSelected;
            return dateFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new DatePickerDialog(Activity, this, currenDate.Year,
                                        currenDate.Month - 1, currenDate.Day);
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            dateSelectedHandler(selectedDate);
        }
    }
}
