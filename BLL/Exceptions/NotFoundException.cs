using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Exceptions
{
    // המבוקש לא נמצא: מועמד, משתמש, פוסט וכדומה
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
