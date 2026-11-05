using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Exceptions
{
    // התנגשות עם מה שכבר קיים: למשל תעודת זהות או מייל שכבר רשומים
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
