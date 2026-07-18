using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Common
{
    public class Result
    {
        protected Result() => _errors = [];



        protected Result(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);

            _errors = [error];
        }
        protected Result(IEnumerable<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            _errors = errors.ToArray();

            if (_errors.Length == 0)
            {
                throw new ArgumentException(
                    "A failed result must contain at least one error.",
                    nameof(errors));
            }
        }



        private readonly Error[] _errors;

        public bool IsSuccess =>_errors.Length == 0;
        public bool IsFailure =>!IsSuccess;

        public IReadOnlyList<Error> Errors => _errors;

        public static Result Success => new();
        public static Result Failure(Error error) => new(error);
        public static Result Failure(List<Error> errors) => new (errors);

    }

    public sealed class Result<TValue> : Result
    {
        private readonly TValue? _value;

        private Result(TValue value)
        {
            _value = value;
        }

        private Result(Error error)
            : base(error)
        {
        }

        private Result(IEnumerable<Error> errors)
            : base(errors)
        {
        }

        public TValue Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException(
                    "The value of a failed result cannot be accessed.");

        public static Result<TValue> Success(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return new Result<TValue>(value);
        }

        public new static Result<TValue> Failure(Error error)
            => new(error);

        public new static Result<TValue> Failure(IEnumerable<Error> errors)
            => new(errors);

        public static implicit operator Result<TValue>(TValue value)
            => Success(value);

        public static implicit operator Result<TValue>(Error error)
            => Failure(error);

        public static implicit operator Result<TValue>(Error[] errors)
            => Failure(errors);
    }
}
