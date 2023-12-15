using System;

namespace GenerationsPOS.PointOfSale.Accounting
{
    public class AccountingException : Exception
    {
        public AccountingException()
        {
        }

        public AccountingException(string? message, Exception? inner) : base(message, inner)
        {
        }
    }

    public class InvoiceCreationException : AccountingException
    {
        public InvoiceCreationException(string message, Exception? inner) : base(message, inner)
        {
        }
    }

    public class InvoicePaymentsException : AccountingException
    {
        public InvoicePaymentsException(string message, Exception? inner) : base(message, inner)
        {
        }
    }
}
