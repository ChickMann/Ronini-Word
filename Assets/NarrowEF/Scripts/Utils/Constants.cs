using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EF.Generic
{
    public class Constants
    {
        // NULL STRINGS
        public const string DB_REFERENCE_NULL = "[CLOUPT] You are trying to get the database reference but this reference is null.";
        public const string AUTH_REFERENCE_NULL = "[CLOUPT] You are trying to get the auth reference but this reference is null.";
    
        // FAILS
        public const string DB_FAIL = "[CLOUPT] There was an error in your database operation.";
        public const string DB_FAIL_CUSTOM_OBJECT = "[CLOUPT] Please try the GetJsonValue<YourClass>(); method..";
        public const string DB_ITEM_VALIDATION_ERROR = "[CLOUPT] Make sure you have configured the project correctly!";
        public const string AUTH_FAIL = "[CLOUPT] There was an error in your AUTH operation.";
        public const string AUTH_USER_NULL = "[CLOUPT] No available user found, please sign in before calling db prefix operation!";
        public const string DATA_EMPTY = "[CLOUPT] Data is empty!";
    
        // MISMATCH
        public const string DATA_MISMATCH = "[CLOUPT] Data type mismatch!";

        // NEW ERROR CONSTANTS
        public const string JSON_CONVERSION_ERROR = "[CLOUPT] Failed to convert JSON to the specified type.";
        public const string DATA_CONVERSION_ERROR = "[CLOUPT] Failed to convert data to the specified type.";
        public const string DB_SET_ERROR = "[CLOUPT] Failed to set data in the database.";
        public const string DB_ADD_ERROR = "[CLOUPT] Failed to add to the existing database value.";
        public const string UNSUPPORTED_DATA_TYPE = "[CLOUPT] Unsupported data type provided for conversion.";
        public const string DB_SNAPSHOT_ERROR = "[CLOUPT] Failed to retrieve a snapshot from the database.";
    }
}