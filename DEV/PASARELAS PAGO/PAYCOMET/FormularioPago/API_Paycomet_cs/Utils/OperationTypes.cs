using System;
namespace API_Paycomet_cs.Models
{
    public class OperationTypes
    {
        public const int ADD_USER = 107;

        public const int EXECUTE_PURCHASE = 1;

        public const int CREATE_SUBSCRIPTION = 9;

        public const int EXECUTE_PURCHASE_TOKEN = 109;

        public const int CREATE_SUBSCRIPTION_TOKEN = 110;

        public const int CREATE_PREAUTHORIZATION = 3;

        public const int PREAUTHORIZATION_CONFIRM = 6;

        public const int PREAUTHORIZATION_CANCEL = 4;

        public const int CREATE_PREAUTHORIZATION_TOKEN = 111;

        public const int DEFERRED_CREATE_PREAUTHORIZATION = 13;

        public const int DEFERRED_PREAUTHORIZATION_CONFIRM = 16;

        public const int DEFERRED_PREAUTHORIZATION_CANCEL = 14;
    }
}
