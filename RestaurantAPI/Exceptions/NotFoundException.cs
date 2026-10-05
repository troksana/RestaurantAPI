using System;

namespace RestaurantAPI.Exceptions
{
    public class NotFoundException : Exception //dziedziczy po exception aby mozna bylo middleware uruchomic w instrukcji throw
    {
        public NotFoundException(string message): base(message) //bazowy message wyjatek z klasy exception
        {

        }
        
    }
}
