using System;
using System.Collections.Generic;
using Razorpay.Api;

class Program {
    static void Main() {
        var x = typeof(Razorpay.Api.Utils).GetMethod("verifyPaymentSignature");
        Console.WriteLine(x.ToString());
    }
}
