﻿// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using MaTech.Common.Algorithm;

namespace MaTech.Common.Data {
    public partial class MetaTable<TEnum> {
        private readonly struct ValueDictHandle {
            private readonly MetaTable<TEnum> owner;
            private readonly IDictionary? inner;
            
            public ValueDictHandle(MetaTable<TEnum> self, IDictionary? dict = null) {
                owner = self;
                inner = dict;
            }

            public bool IsValid => owner != null;
            public bool HasValueDict => inner != null;
            public bool HasValueDictOfType<T>() => inner is Dictionary<EnumEx<TEnum>, T>;
            public Type? TypeOfValueDict => inner?.GetType();
            
            public ValueDict<T> Dereference<T>() {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (!IsValid) {
                    throw new NotImplementedException("[MetaTable] Dereference on a invalid ValueDictHandle. The logic is implemented wrong.");
                }
                if (HasValueDict && !HasValueDictOfType<T>()) {
                    throw new NotImplementedException($"[MetaTable] Dereference on a wrong typed ValueDict. Expected type [{typeof(ValueDict<T>).FullName}], got type [{TypeOfValueDict?.FullName}].");
                }
                #endif
                
                return new ValueDict<T>((Dictionary<EnumEx<TEnum>, T>?)inner);
            }
        }
        
        private readonly struct ValueDict<T> {
            private readonly Dictionary<EnumEx<TEnum>, T>? inner;
            
            public ValueDict(Dictionary<EnumEx<TEnum>, T>? dict) { inner = dict; }
            public static ValueDict<T> Empty => new ValueDict<T>(null);

            // non-value methods -- test nullable
            public bool Has(EnumEx<TEnum> key) => inner?.ContainsKey(key) ?? false;
            public bool TryGet(EnumEx<TEnum> key, out T? result) => inner != null ? inner.TryGetValue(key, out result) : ((result = default), false).Item2;
            public bool Remove(EnumEx<TEnum> key) => inner?.Remove(key) ?? false;
            
            public int Collect(ICollection<T> outList) {
                if (inner == null) return 0;
                foreach (var value in inner.Values) {
                    outList.Add(value);
                }
                return inner.Count;
            }

            // value methods -- assert not null
            public T Set(EnumEx<TEnum> key, in T value) => inner![key] = value;
            public T GetOrSet(EnumEx<TEnum> key, in T value) => inner!.GetOrAdd(key, value);
        }

        private readonly Dictionary<Type, ValueDictHandle> dictHandlesByType = new Dictionary<Type, ValueDictHandle>();
        
        private ValueDict<T> ValueDictOf<T>() => ValueDictHandleOf<T>().Dereference<T>();
        private ValueDict<T> ValueDictOf<T>(EnumEx<TEnum> keyToCheck) {
            if (!CheckConstraintTypeOfKey<T>(keyToCheck)) return ValueDict<T>.Empty;
            return ValueDictHandleOf<T>().Dereference<T>();
        }

        private ValueDict<T> ValueDictOf<T>(EnumEx<TEnum> keyToCheck, in T? valueToCheck) {
            if (!CheckConstraintTypeOfKey<T>(keyToCheck)) return ValueDict<T>.Empty;
            if (!CheckConstraintValueOfKey<T>(keyToCheck, valueToCheck)) return ValueDict<T>.Empty;
            return WritableValueDictHandleOf<T>().Dereference<T>();
        }

        private ValueDictHandle ValueDictHandleOf<T>() {
            Type type = typeof(T);
            if (!dictHandlesByType.TryGetValue(type, out var handle)) {
                dictHandlesByType[type] = handle = new ValueDictHandle(this);
            }
            return handle;
        }
        
        private ValueDictHandle WritableValueDictHandleOf<T>() {
            Type type = typeof(T);
            if (!dictHandlesByType.TryGetValue(type, out var handle) || !handle.HasValueDict) {
                dictHandlesByType[type] = handle = new ValueDictHandle(this, new Dictionary<EnumEx<TEnum>, T>());
            }
            return handle;
        }

        // TODO: check boxing on enum keys and fix it with custom IEqualityComparer (hashcode from EnumEx)
    }
}