using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Exceptions
{
    // הפרה של כלל במערכת: למשל ניסיון לבצע פעולה שלא מתאימה לפי הכללים
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) { }
    }
}
