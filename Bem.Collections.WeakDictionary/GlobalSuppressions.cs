// --------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "<Pending>")]
[assembly: SuppressMessage("Critical Code Smell", "S1067:Expressions should not be too complex", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.WeakDictionary`2.HashMemoWeakReferenceEqualityComparer`1.Equals(Bem.Collections.WeakDictionary.WeakDictionary`2.HashMemoWeakReference{`2},Bem.Collections.WeakDictionary.WeakDictionary`2.HashMemoWeakReference{`2})~System.Boolean")]
[assembly: SuppressMessage("Info Code Smell", "S1309:Track uses of in-source issue suppressions", Justification = "<Pending>")]
[assembly: SuppressMessage("Minor Bug", "S1206:\"Equals(Object)\" and \"GetHashCode()\" should be overridden in pairs", Justification = "There is no possible execution path where Equals method is called on this type", Scope = "type", Target = "~T:Bem.Collections.WeakDictionary.WeakDictionary`2.HashMemoWeakReference`1")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.AddOrUpdate``2(System.Collections.Generic.IDictionary{``0,``1},``0,System.Func{``0,``1},System.Func{``0,``1,``1})~``1")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<Pending>", Scope = "type", Target = "~T:Bem.Collections.WeakDictionary.DictionaryExtensions")]
[assembly: SuppressMessage("Roslynator", "RCS1165:Unconstrained type parameter checked for null.", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.AddOrUpdate``2(System.Collections.Generic.IDictionary{``0,``1},``0,System.Func{``0,``1},System.Func{``0,``1,``1})~``1")]
[assembly: SuppressMessage("Minor Bug", "S2955:Generic parameters not constrained to reference types should not be compared to \"null\"", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.AddOrUpdate``2(System.Collections.Generic.IDictionary{``0,``1},``0,System.Func{``0,``1},System.Func{``0,``1,``1})~``1")]
[assembly: SuppressMessage("Roslynator", "RCS1165:Unconstrained type parameter checked for null.", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.GetOrAdd``2(System.Collections.Generic.IDictionary{``0,``1},``0,System.Func{``0,``1})~``1")]
[assembly: SuppressMessage("Minor Bug", "S2955:Generic parameters not constrained to reference types should not be compared to \"null\"", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.GetOrAdd``2(System.Collections.Generic.IDictionary{``0,``1},``0,System.Func{``0,``1})~``1")]
[assembly: SuppressMessage("Roslynator", "RCS1165:Unconstrained type parameter checked for null.", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.TryRemove``2(System.Collections.Generic.IDictionary{``0,``1},``0,``1@)~System.Boolean")]
[assembly: SuppressMessage("Minor Bug", "S2955:Generic parameters not constrained to reference types should not be compared to \"null\"", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.DictionaryExtensions.TryRemove``2(System.Collections.Generic.IDictionary{``0,``1},``0,``1@)~System.Boolean")]
