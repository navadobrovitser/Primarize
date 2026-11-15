using System;

namespace BLL.Services
{
    // =====================================================
    // חריגה ייעודית לתקלות בשירות הבינה המלאכותית.
    //
    // מאפשרת ל-Controller להבדיל בין:
    // תקלה ב-Gemini לבין תקלה במאגר התקנון.
    // =====================================================

    public class RegulationAiUnavailableException : Exception
    {
        public RegulationAiUnavailableException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}