using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SQLiteDemo
{
    
    public class NoteBase { }

    [SugarTable("Note")]
    public class Note : NoteBase
    {
        #region Data properties
        // Setting IsNullable is required or the table won't be created

        /// <summary>
        /// NoteID: self incremented, maintained by database
        /// </summary>
        [SugarColumn(
            IsNullable = false,
            ColumnName = "NoteID",
            IsIdentity = true,
            IsPrimaryKey = true
            )]
        public int NoteID
        { get; set; }

        [SugarColumn(
            IsNullable = false,             
            ColumnName = "Title")]
        public string Title
        { get; set; }

        [SugarColumn(IsNullable = false, ColumnName = "Content")]
        public string Content
        { get; set; }

        [SugarColumn(IsNullable = false, ColumnName = "Directory")]
        public string Directory
        { get; set; }

        #endregion

        #region Overriden object methods
        public static bool operator ==(Note lhs, Note rhs)
            => (lhs?.Title, lhs?.Directory) == (rhs?.Title, rhs?.Directory);

        public static bool operator !=(Note lhs, Note rhs)
            => (lhs?.Title, lhs?.Directory) != (rhs?.Title, rhs?.Directory);

        public override int GetHashCode()
            => NoteID.GetHashCode() ^ Title.GetHashCode() ^ Directory.GetHashCode();

        public override bool Equals(object obj)
            => (obj is Note note)
            ? this == note
            : false;

        #endregion

        #region Constant           
        public static readonly Note VoidNote = new Note
        {
            Title = string.Empty,
            Content = string.Empty,
            Directory = string.Empty
        };
        #endregion
    }

    [SugarTable("KnowledgeBase")]
    public class KnowledgeBase : NoteBase
    {
        #region Data properties
        [SugarColumn(
             IsNullable = false,
             IsIdentity = true,
             IsPrimaryKey = true
             )]
        public int KnowledgeBaseID
        { get; set; }

        [SugarColumn(IsNullable = false//, IsPrimaryKey = true
            )]
        public string Name
        { get; set; }

        #endregion

        #region Overriden object methods

        public static bool operator ==(KnowledgeBase lhs, KnowledgeBase rhs)
            => lhs?.Name == rhs?.Name;
        public static bool operator !=(KnowledgeBase lhs, KnowledgeBase rhs)
            => lhs?.Name != rhs?.Name;
         
        public override int GetHashCode()
            => Name.GetHashCode();

        public override bool Equals(object obj)
            => (obj is KnowledgeBase kbase)
            ? this == kbase
            : false;

        #endregion

        #region Constant
        public static readonly KnowledgeBase VoidKnowledgeBase = new KnowledgeBase
        {
            Name = string.Empty
        };
        #endregion

        #region Member methods

        #endregion

    }



}
