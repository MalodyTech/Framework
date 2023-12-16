﻿// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;

namespace MaTech.Common.Data {
    public interface IMetaMethods<TKey> where TKey : unmanaged {
        bool Has(in TKey key) => !Get(key).IsNone;
        Variant Get(in TKey key);
    }

    public interface IMetaTableMethods<TKey> where TKey : unmanaged {
        bool Remove(in TKey key);
        bool TrySet(in TKey key, in Variant value, bool overwrite = true);
    }

    public interface IMetaVisitableMethods<TKey> where TKey : unmanaged {
        public interface IVisitor {
            Variant Visit(in TKey key, in Variant value);
        }
        void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IVisitor;
    }

    public interface IMeta : IMetaMethods<MetaEnum> { }
    public interface IMeta<TEnum> : IMetaMethods<EnumEx<TEnum>> where TEnum : unmanaged, Enum, IConvertible { }
    public interface IMetaTable : IMeta, IMetaTableMethods<MetaEnum> { }
    public interface IMetaTable<TEnum> : IMeta<TEnum>, IMetaTableMethods<EnumEx<TEnum>> where TEnum : unmanaged, Enum, IConvertible { }
    public interface IMetaVisitable : IMetaVisitableMethods<MetaEnum> { }
    public interface IMetaVisitable<TEnum> : IMetaVisitableMethods<EnumEx<TEnum>> where TEnum : unmanaged, Enum, IConvertible { }
}