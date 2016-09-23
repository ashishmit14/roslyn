﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Editor.Implementation.NavigateTo;
using Microsoft.CodeAnalysis.NavigateTo;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Remote.Arguments
{
    #region Common Arguments

    /// <summary>
    /// Arguments to pass from client to server when performing operations
    /// </summary>
    internal class SerializableProjectId
    {
        public Guid Id;
        public string DebugName;

        public static SerializableProjectId Dehydrate(ProjectId id)
        {
            return new SerializableProjectId { Id = id.Id, DebugName = id.DebugName };
        }

        public ProjectId Rehydrate()
        {
            return ProjectId.CreateFromSerialized(Id, DebugName);
        }
    }

    internal class SerializableDocumentId
    {
        public SerializableProjectId ProjectId;
        public Guid Id;
        public string DebugName;

        public static SerializableDocumentId Dehydrate(Document document)
        {
            return Dehydrate(document.Id);
        }

        public static SerializableDocumentId Dehydrate(DocumentId id)
        {
            return new SerializableDocumentId
            {
                ProjectId = SerializableProjectId.Dehydrate(id.ProjectId),
                Id = id.Id,
                DebugName = id.DebugName
            };
        }

        public DocumentId Rehydrate()
        {
            return DocumentId.CreateFromSerialized(
                ProjectId.Rehydrate(), Id, DebugName);
        }
    }

    internal class SerializableTextSpan
    {
        public int Start;
        public int Length;

        public static SerializableTextSpan Dehydrate(TextSpan textSpan)
        {
            return new SerializableTextSpan { Start = textSpan.Start, Length = textSpan.Length };
        }

        public TextSpan Rehydrate()
        {
            return new TextSpan(Start, Length);
        }
    }

    internal class SerializableTaggedText
    {
        public string Tag;
        public string Text;

        public static SerializableTaggedText Dehydrate(TaggedText taggedText)
        {
            return new SerializableTaggedText { Tag = taggedText.Tag, Text = taggedText.Text };
        }

        public TaggedText Rehydrate()
        {
            return new TaggedText(Tag, Text);
        }
    }

    #endregion

    #region NavigateTo

    internal class SerializableNavigateToSearchResult
    {
        public string AdditionalInformation;

        public string Kind;
        public NavigateToMatchKind MatchKind;
        public bool IsCaseSensitive;
        public string Name;
        public string SecondarySort;
        public string Summary;

        public SerializableNavigableItem NavigableItem;

        internal INavigateToSearchResult Rehydrate(Solution solution)
        {
            return new NavigateToSearchResult(
                AdditionalInformation, Kind, MatchKind, IsCaseSensitive,
                Name, SecondarySort, Summary, NavigableItem.Rehydrate(solution));
        }

        private class NavigateToSearchResult : INavigateToSearchResult
        {
            public string AdditionalInformation { get; }
            public string Kind { get; }
            public NavigateToMatchKind MatchKind { get; }
            public bool IsCaseSensitive { get; }
            public string Name { get; }
            public string SecondarySort { get; }
            public string Summary { get; }

            public INavigableItem NavigableItem { get; }

            public NavigateToSearchResult(string additionalInformation, string kind, NavigateToMatchKind matchKind, bool isCaseSensitive, string name, string secondarySort, string summary, INavigableItem navigableItem)
            {
                AdditionalInformation = additionalInformation;
                Kind = kind;
                MatchKind = matchKind;
                IsCaseSensitive = isCaseSensitive;
                Name = name;
                SecondarySort = secondarySort;
                Summary = summary;
                NavigableItem = navigableItem;
            }
        }
    }

    internal class SerializableNavigableItem
    {
        public Glyph Glyph;

        public SerializableTaggedText[] DisplayTaggedParts;

        public bool DisplayFileLocation;

        public bool IsImplicitlyDeclared;

        public SerializableDocumentId Document;
        public SerializableTextSpan SourceSpan;

        SerializableNavigableItem[] ChildItems;

        public INavigableItem Rehydrate(Solution solution)
        {
            var childItems = ChildItems == null
                ? ImmutableArray<INavigableItem>.Empty
                : ChildItems.Select(c => c.Rehydrate(solution)).ToImmutableArray();
            return new NavigableItem(
                Glyph, DisplayTaggedParts.Select(p => p.Rehydrate()).ToImmutableArray(),
                DisplayFileLocation, IsImplicitlyDeclared,
                solution.GetDocument(Document.Rehydrate()),
                SourceSpan.Rehydrate(),
                childItems);
        }

        private class NavigableItem : INavigableItem
        {
            public Glyph Glyph { get; }
            public ImmutableArray<TaggedText> DisplayTaggedParts { get; }
            public bool DisplayFileLocation { get; }
            public bool IsImplicitlyDeclared { get; }

            public Document Document { get; }
            public TextSpan SourceSpan { get; }

            public ImmutableArray<INavigableItem> ChildItems { get; }

            public NavigableItem(
                Glyph glyph, ImmutableArray<TaggedText> displayTaggedParts, 
                bool displayFileLocation, bool isImplicitlyDeclared, Document document, TextSpan sourceSpan, ImmutableArray<INavigableItem> childItems)
            {
                Glyph = glyph;
                DisplayTaggedParts = displayTaggedParts;
                DisplayFileLocation = displayFileLocation;
                IsImplicitlyDeclared = isImplicitlyDeclared;
                Document = document;
                SourceSpan = sourceSpan;
                ChildItems = childItems;
            }
        }
    }

    #endregion
}