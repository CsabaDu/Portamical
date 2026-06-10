// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Formatting.Model;

public abstract class Formatter<T> : IFormatter<T>
{
    public abstract string? Format(T value);

    string? IFormatter.Format(object value)
    => Format((T)value);

    protected static string? Format<TContext>(
        Func<TContext, string?> toString,
        TContext context)
    => toString(context);
}
