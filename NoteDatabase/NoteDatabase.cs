using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace SQLiteDemo
{
   
    public class NoteDatabase
    {
        #region Constructor
        /// <summary>
        /// NoteDatabase 构造方法:
        /// 若数据库文件不存在则在磁盘上新建一个数据库实例，
        /// 并创建KnowledgeBase和Note两张表。
        /// </summary>
        private static readonly string DatabaseFile = "NoteDB.db";
        public NoteDatabase()
        {

            // Check database existence
            if (File.Exists(DatabaseFile))
            {
                File.Delete(DatabaseFile);
            }

            // Connect to a new created Sqlite instance
            // CAUTION: CANNOT HANDLE IN-MEMORY INSTANCE!
            database = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "Data Source=" + DatabaseFile,
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            // Create tables of Note and KnowledgeBase
            database.CodeFirst.InitTables(typeof(Note));
            database.CodeFirst.InitTables(typeof(KnowledgeBase));

            // Test table creation
            try
            { 
                var all = database.Queryable<Note>().ToList();
                var kb = database.Queryable<KnowledgeBase>().InSingle("Math");
            }
            catch (Exception InitTableFailure)
            {
                Console.WriteLine(InitTableFailure.Message);
                database.Close();
            }

        }
        #endregion

        #region Query
        /// <summary>
        /// 查找并返回名为kbName的知识库。
        /// 返回：
        ///     成功则返回对应名字的知识库；
        ///     如果kbName为null或空则返回null;
        ///     如果该知识库不存在则返回VoidKnowledgeBase。        
        /// </summary>
        /// <param name="kbName"></param>
        /// <returns></returns>
        public KnowledgeBase FetchKnowledgeBase(string kbName)
        {
            // Validate input
            if (string.IsNullOrEmpty(kbName))
                return null;

            // Query by the primary key
            var kbList = database.Queryable<KnowledgeBase>().Where(kb => kb.Name == kbName).ToList();
            if (kbList.Count > 1 )
            {
                throw new Exception("Duplicate KnowledgeBase!");
            }

            // Return the KnowledgeBase given the name;
            // return VoidKnowledgebase if no such KnowledgeBase exists
            return kbList.Count == 0
                ? KnowledgeBase.VoidKnowledgeBase
                : kbList[0];
        }

        /// <summary>
        /// 查找并返回数据库中所有的知识库。
        /// 返回：若数据库中没有知识库则返回空列表。
        /// </summary>
        /// <returns></returns>
        public List<KnowledgeBase> FetchKnowledgeBase()
            => database.Queryable<KnowledgeBase>().ToList();
        

        /// <summary>
        /// 查找并返回知识库kbName内，标题为noteTitle的笔记页
        /// 返回： 成功则返回对应笔记页；
        ///     如果不存在该笔记页则返回VoidNote；
        ///     如果参数为null/空，或查找失败，则返回null
        /// </summary>
        /// <param name="noteTitle"></param>
        /// <returns></returns>
        public Note FetchNote(string kbName, string noteTitle)
        {
            // Validate input
            if (string.IsNullOrEmpty(kbName) 
                || string.IsNullOrEmpty(noteTitle))
                return null;

            // Query by the title and directory            
            var result = database.Queryable<Note>().Where(
                  note => note.Title == noteTitle && note.Directory == kbName
                  ).ToList();      
                      
            // Validate result
            try
            {
                switch (result.Count)
                {
                    // Return the Note found given the directory and the title
                    // Success!
                    case 1:
                        return result[0];
                    // Return VoidNote if no such knowledgebase exists 
                    case 0:
                        return Note.VoidNote;  
                    // Throw Exception if duplicate notes are found
                    default:
                        throw new Exception("FetchNote: Duplicate notes!");
                }
            }
            // Print duplicates
            catch (Exception FetchNoteFailure)
            {
                Console.WriteLine(FetchNoteFailure.Message);
                foreach (var n in result)
                {
                    Console.WriteLine($"{n.Title} {n.Directory}");
                }
                return null;
            }            
        }

        /// <summary>
        /// 查找并返回kbName知识库下所有笔记
        /// 返回：若成功则返回知识库内所有笔记页；
        ///     如果参数为null/空，或该知识库不存在则返回null
        /// </summary>
        /// <param name="kbName"></param>
        /// <returns></returns>
        public List<Note> FetchNote(string kbName)
        {
            if (string.IsNullOrEmpty(kbName))
            { 
                return null;
            }
            var result = database.Queryable<Note>().Where(note => note.Directory == kbName);
            return result == null
                ? new List<Note>()
                : result.ToList();
        }

        /// <summary>
        /// 判断一个知识库对象（KnowledgeBase）是否已经存在于当前知识库列表（knowledgeBases）中
        /// </summary>
        /// <param name="kbName"></param>
        /// <returns></returns>
        public bool ContainsKnowledgeBase(string kbName)
        {
            if (string.IsNullOrEmpty(kbName))
            {
                return false;
            }
            var result = database.Queryable<KnowledgeBase>().Where(kb => kb.Name == kbName);
            return result == null
                ? false
                : true;
        }

        /// <summary>
        /// 判断名为kbName的知识库中是否包含标题为noteTitle的笔记页
        /// </summary>
        /// <param name="kbName"></param>
        /// <param name="noteTitle"></param>
        /// <returns></returns>
        public bool ContainsNote(string kbName, string noteTitle)
        {
            // Validate input
            if (string.IsNullOrEmpty(noteTitle))
                return false;

            // Query by the title and directory            
            var result = database.Queryable<Note>().Where(
                  note => note.Title == noteTitle && note.Directory == kbName
                  ).ToList();

            // Validate result
            return result.Count > 0
                ? true
                : false;
        }

        /// <summary>
        /// 判断该笔记页是否在当前列表中任意一个知识库中。
        /// 若存在，则返回true
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        // 
        public bool ContainsNote(string noteTitle)
        {
            // Validate input
            if (string.IsNullOrEmpty(noteTitle))
                return false;

            // Query by the title and directory            
            var result = database.Queryable<Note>().Where(
                  note => note.Title == noteTitle
                  ).ToList();

            // Validate result
            return result.Count > 0
                ? true
                : false;
        }

        #endregion

        #region Insert
        /// <summary>
        /// Add a KnowledgeBase
        /// No duplicate names allowed
        /// </summary>
        /// <param name="kbase"></param>
        /// <returns></returns>
        public bool addKB(KnowledgeBase kbase)
        {
            // Validate input
            if (kbase == null)
                return false;

            // Add if not exists
            if (ContainsKnowledgeBase(kbase.Name))
            {
                return false;
            }
            try
            {
                database.Insertable(kbase).ExecuteCommand();
            }
            catch (Exception InsertKnowledgeBaseFailure)
            {
                Console.Write(InsertKnowledgeBaseFailure.Message);
                Console.WriteLine(", press any key to continue:");
                Console.ReadKey();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add a Note
        /// No duplicate names allowed
        /// </summary>
        /// <param name="kbase"></param>
        /// <returns></returns>
        public bool addNote(Note note)
        {
            // Validate input
            if (note == null)
                return false;

            // Add if not exists
            if (ContainsNote(note.Directory, note.Title))
            {
                throw new Exception("addKB: 重名笔记页已经存在！");
            }
            try
            {
                database.Insertable(note).ExecuteCommand();
            }
            catch (Exception InsertNoteFailure)
            {
                Console.Write(InsertNoteFailure.Message);
                Console.WriteLine(", press any key to continue:");
                Console.ReadKey();
                return false;
            }
            return true;
        }
        #endregion

        #region Modifier

        /// <summary>
        /// 更新一个KnowledgeBase的记录。
        /// 若成功则执行一次查找，返回数据库内更新后的KnowledgeBase。
        /// </summary>
        /// <param name="latest"></param>
        /// <returns></returns>
        public KnowledgeBase UpdateKnowledgeBase(KnowledgeBase latest)
        {
            // Validate input
            // Check if the record actually exists
            var q = database.Queryable<KnowledgeBase>().InSingle(latest.KnowledgeBaseID);
            if (latest == null 
                || q == null)
            {
                return null;
            }

            // Update and check result
            var affected = database.Updateable(latest).ExecuteCommand();
            if (affected != 1)
            {
                throw new Exception($"UpdateKnowledgeBase {latest.Name} failed!");
            }

            // Return the updated version of the record
            return FetchKnowledgeBase(latest.Name);
        }

        /// <summary>
        /// 更新一个Note的记录。
        /// 若成功则执行一次查找，返回数据库内更新后的Note。
        /// </summary>
        /// <param name="latest"></param>
        /// <returns></returns>
        public Note UpdateNote(Note latest)
        {
            // Validate input
            // Check if the record actually exists
            if (latest == null
                || FetchNote(latest.Directory, latest.Title) == Note.VoidNote)
            {
                return null;
            }

            // Update and check result
            var affected = database.Updateable(latest).ExecuteCommand();
            if (affected != 1)
            {
                //throw new Exception("UpdateNote: Multiple columns affected!");
            }

            // Return the updated version of the record
            return FetchNote(latest.Directory, latest.Title);
        }

        /// <summary>
        /// 从数据库中删除名为name的知识库，并将其返回
        /// </summary>
        /// <param name="kbName"></param>
        /// <returns></returns>
        public KnowledgeBase DeleteKnowledgeBase(string kbName)
        {
            // Validate input
            var toBeDeleted = FetchKnowledgeBase(kbName);
            if (toBeDeleted == KnowledgeBase.VoidKnowledgeBase)
            {
                return toBeDeleted;
            }

            // Delete
            var affected = database.Deleteable<KnowledgeBase>().Where(kb => kb.Name == kbName).ExecuteCommand();                  
            if (affected != 1)
            {
                throw new Exception($"DeleteKnowledgeNote {kbName} failed!");
            }
            return toBeDeleted;
        }

        /// <summary>
        /// 从数据库中删除知识库directory内标题为title的笔记页，并将其返回
        /// </summary>
        /// <param name="kbName"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public Note DeleteNote(string kbName, string title)
        {
            // Validate input
            var toBeDelete = FetchNote(kbName, title);
            if (toBeDelete == Note.VoidNote)
            {
                return toBeDelete;
            }

            // Delete
            var affected = database.Deleteable<Note>().Where(toBeDelete).ExecuteCommand();
            if (affected != 1)
            {
                throw new Exception($"DeleteNote {kbName}.{title} failed");
            }
            return toBeDelete;
        }

        #endregion

        #region Data field
        private SqlSugarClient database;
        #endregion
    }
}
