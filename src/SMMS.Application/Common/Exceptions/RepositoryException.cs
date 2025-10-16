using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Exceptions;
public sealed class RepositoryException : Exception
{
    public string Repository { get; }
    public string Operation { get; }

    public RepositoryException(string repository, string operation, string message, Exception inner)
        : base(message, inner)
    {
        Repository = repository;
        Operation = operation;
    }
}
