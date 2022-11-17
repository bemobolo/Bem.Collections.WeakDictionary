// --------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Daniel Balogh">
//     Copyright (c) Daniel Balogh. All rights reserved.
//     Licensed under the GNU Generic Public License 3.0 license.
//     See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "<Pending>", Scope = "type", Target = "~T:Bem.Collections.WeakDictionary.Tests.WeakDictionaryPerformanceTests")]
[assembly: SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "<Pending>")]
[assembly: SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "<Pending>", Scope = "type", Target = "~T:Bem.Collections.WeakDictionary.Tests.WeakDictionaryTests")]
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "<Pending>", Scope = "member", Target = "~M:Bem.Collections.WeakDictionary.Tests.WeakDictionaryTests.Values_Returns_Values_For_Entries_Where_Key_Is_Not_GCd~System.Threading.Tasks.Task")]
