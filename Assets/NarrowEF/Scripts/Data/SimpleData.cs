using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EF.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace EF.Database
{
    /// <summary>
    /// Class used for simple data operations.
    /// </summary>
    [Serializable]
    public class SimpleData
    {
        // Selected tag
        public string selectedTag;
        // Prefix
        public string prefix { get; private set; }

        // Current data type
        public DataType currentDataType;

        private bool validated;

        public SimpleData(string tag, DataType type, string pre)
        {
            if(tag==null || pre==null ) return;
            selectedTag = tag;
            currentDataType =type;
            prefix = pre;
            validated = true;
        }

        #region Handle

        /// <summary>
        /// Determines the data type based on the selected tag.
        /// </summary>
        private void Validate()
        {
            try
            {
                DataItem item = EFSettings.Instance.dataItems.FirstOrDefault(d => d.title == selectedTag);
                if (item != null)
                {
                    currentDataType = item.dataType;
                    prefix = item.prefix;
                    validated = true;
                }
                else
                {
                    throw new EFException(Constants.DB_ITEM_VALIDATION_ERROR);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw new EFException(Constants.DB_ITEM_VALIDATION_ERROR);
            }
        }

     
        #endregion

        #region Retrieve

        /// <summary>
        /// Retrieves the JSON value and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <returns>The converted data</returns>
        public async Task<T> GetJsonValue<T>()
        {
            var snapshot = await GetSnapshot();

            // [FIX] Nếu data chưa có -> Trả về null hoặc default để Manager tự tạo mới
            if (snapshot == null || !snapshot.Exists)
            {
                return default(T);
            }

            string json = snapshot.GetRawJsonValue();
    
            if (string.IsNullOrEmpty(json)) return default(T);

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonSerializationException)
            {
                // [QUAN TRỌNG] Bắt lỗi "Mảng ma quái"
                // Nếu Firebase trả về Array [] nhưng code đòi Dictionary {}
                // Ta trả về default (null) để code bên ngoài tự khởi tạo Dictionary rỗng.
                Debug.LogWarning("[SimpleData] Phát hiện dữ liệu dạng Array (cũ) thay vì Dictionary. Đang reset...");
                return default(T);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SimpleData] GetJson Error: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Retrieves the data and converts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <returns>The converted data</returns>
        public async Task<T> GetData<T>()
        {
            var snapshot = await GetSnapshot();

            // [FIX] Kiểm tra nếu data không tồn tại hoặc null -> Trả về giá trị mặc định luôn
            if (snapshot == null || !snapshot.Exists || snapshot.Value == null)
            {
                return default(T);
            }

            try
            {
                // Dùng hàm ConvertTo an toàn đã viết ở bước trước
                return ConvertTo<T>(snapshot.Value.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SimpleData] GetData Error: {ex.Message}. Returning default.");
                return default(T);
            }
        }

        #endregion

        #region Set Data

        /// <summary>
        /// Sets the data in the database.
        /// </summary>
        /// <typeparam name="T">The type of data to set</typeparam>
        /// <param name="data">The data to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SetData<T>(T data)
        {
            try
            {
                if (!validated) Validate();

                var json = JsonConvert.SerializeObject(data);
                await EFManager.Instance.GetReference()
                    .Child(EFManager.Instance.RePlacePrefix(prefix))
                    .Child(selectedTag)
                    .SetRawJsonValueAsync(json);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw new EFException(Constants.DB_SET_ERROR);
            }
        }

        /// <summary>
        /// Sets a string value in the database.
        /// </summary>
        /// <param name="value">The string value to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<string> SetString(string value)
        {
            try
            {
                if (currentDataType != DataType.String)
                {
                    throw new EFException(Constants.DATA_MISMATCH);
                }
                await SetData(value);

                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Sets an integer value in the database.
        /// </summary>
        /// <param name="value">The integer value to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<int> SetInteger(int value)
        {
            try
            {
                Debug.Log(currentDataType);
                if (currentDataType != DataType.Integer)
                {
                    throw new EFException(Constants.DATA_MISMATCH);
                }
                await SetData(value);

                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Sets a boolean value in the database.
        /// </summary>
        /// <param name="value">The boolean value to set</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<bool> SetBoolean(bool value)
        {
            try
            {
                if (currentDataType != DataType.Bool)
                {
                    throw new EFException(Constants.DATA_MISMATCH);
                }
                await SetData(value);

                return value;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        #endregion

        #region Add Data

        /// <summary>
        /// Adds an integer value to the existing integer value in the database.
        /// </summary>
        /// <param name="value">The integer value to add</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<int> AddInteger(int value)
        {
            try
            {
                if (!validated) Validate();
                if (currentDataType != DataType.Integer)
                {
                    throw new EFException(Constants.DATA_MISMATCH);
                }
                var currentValue = await GetData<int>();
                var newValue = currentValue + value;

                await SetInteger(newValue);

                return newValue;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw new EFException(Constants.DB_ADD_ERROR);
            }
        }

        #endregion

        #region Convert

        /// <summary>
        /// Converts the given string value to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="value">The string value to convert</param>
        /// <returns>The converted data</returns>
        private T ConvertTo<T>(string value)
        {
            try
            {
                Type t = typeof(T);
        
                // Trường hợp 1: Xử lý số nguyên (int hoặc int?)
                if (t == typeof(int) || t == typeof(int?))
                {
                    if (string.IsNullOrEmpty(value)) return (T)(object)0;
                    if (int.TryParse(value, out int result)) return (T)(object)result;
                    return (T)(object)0; // Fallback an toàn
                }

                // Trường hợp 2: Xử lý số thực (float hoặc float?)
                if (t == typeof(float) || t == typeof(float?))
                {
                    if (string.IsNullOrEmpty(value)) return (T)(object)0f;
                    if (float.TryParse(value, out float result)) return (T)(object)result;
                    return (T)(object)0f;
                }

                // Trường hợp 3: Xử lý Boolean
                if (t == typeof(bool) || t == typeof(bool?))
                {
                    if (bool.TryParse(value, out bool result)) return (T)(object)result;
                    return (T)(object)false;
                }

                // Trường hợp 4: Chuỗi JSON hoặc String thường
                return (T)(object)value;
            }
            catch
            {
                // Nếu mọi thứ thất bại, trả về giá trị mặc định thay vì Crash game
                return default(T);
            }
        }

        #endregion

        #region Firebase

        /// <summary>
        /// Retrieves a snapshot of the data from Firebase.
        /// </summary>
        /// <returns>The data snapshot</returns>
        public async Task<DataSnapshot> GetSnapshot()
        {
            try
            {
                DataSnapshot snapshot = await EFManager.Instance.GetReference()
                    .Child(EFManager.Instance.RePlacePrefix(prefix))
                    .Child(selectedTag)
                    .GetValueAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled || task.IsFaulted)
                        {
                            throw new EFException(Constants.DB_FAIL);
                        }

                        return task.Result;
                    });

                return snapshot;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw new EFException(Constants.DB_SNAPSHOT_ERROR);
            }
        }

        #endregion
    }
}
