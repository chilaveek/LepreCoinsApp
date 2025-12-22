using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? ErrorCode { get; private set; }

        protected Result(bool isSuccess, string? errorMessage = null, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static Result Success() => new(true);
        public static Result Failure(string errorMessage, string? errorCode = null)
            => new(false, errorMessage, errorCode);

        public static Result<T> Success<T>(T data) => new(true, data);
        public static Result<T> Failure<T>(string errorMessage, string? errorCode = null)
            => new(false, default, errorMessage, errorCode);
    }

    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        public Result(bool isSuccess, T? data, string? errorMessage = null, string? errorCode = null)
            : base(isSuccess, errorMessage, errorCode)
        {
            Data = data;
        }

        public new static Result<T> Success(T data) => new(true, data);
        public new static Result<T> Failure(string errorMessage, string? errorCode = null)
            => new(false, default, errorMessage, errorCode);
    }

}
