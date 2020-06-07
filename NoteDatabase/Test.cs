using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDemo
{
    public class TestHelper
    {   
        public static List<KnowledgeBase> CreateTestKnowledgeBase()
        {
            // create some kbases
            List<KnowledgeBase> kbList = new List<KnowledgeBase>();
            kbList.Add(new KnowledgeBase { Name = "Math" });
            kbList.Add(new KnowledgeBase { Name = "English" });
            kbList.Add(new KnowledgeBase { Name = "Cpp" });

            return kbList;
        }

        public static void AddKnowledgeBases(NoteDatabase db, List<KnowledgeBase> kbList)
        {
            foreach (var kb in kbList)
            {
                db.addKB(kb);
                Console.WriteLine($"Add kbase {kb.Name}\n");
            }
        }

        public static List<Note> CretaTestNotes()
        {
            // create some notes
            List<Note> noteList = new List<Note>();
            noteList.Add(new Note
            {
                Title = "Triangle",
                Content = "",
                Directory = "Math"
            });
            noteList.Add(new Note
            {
                Title = "Calculas",
                Content = "",
                Directory = "Math"
            });
            noteList.Add(new Note
            {
                Title = "Matrix",
                Content = "",
                Directory = "Math"
            });
            noteList.Add(new Note
            {
                Title = "Grammar",
                Content = "",
                Directory = "English"
            });
            noteList.Add(new Note
            {
                Title = "Culture",
                Content = "",
                Directory = "English"
            });
            noteList.Add(new Note
            {
                Title = "Pointer",
                Content = "",
                Directory = "Cpp"
            });
            noteList.Add(new Note
            {
                Title = "C++11",
                Content = "",
                Directory = "CPP"
            });
            noteList.Add(new Note
            {
                Title = "Template",
                Content = "",
                Directory = "CPP"
            });

            return noteList;
        }

        public static void AddNotes(NoteDatabase db, List<Note> noteList)
        {
            foreach (var note in noteList)
            {
                db.addNote(note);
                Console.WriteLine($"Add note {note.Title}\n");
            }
        }
    }

    public class Test
    {
        public static void Main(string[] args)
        {
            #region Arrange Test
            // create db
            var db = new NoteDatabase();

            // create test knowledgebases
            var kbList = TestHelper.CreateTestKnowledgeBase();

            // add test knowledgebases
            TestHelper.AddKnowledgeBases(db, kbList);

            // create test notes
            var noteList = TestHelper.CretaTestNotes();

            // add test notes
            TestHelper.AddNotes(db, noteList);

            #endregion

            #region Test Fetch
            // Test 1
            TestFetchKnowledgebase(db, kbList);

            // Test 2
            TestFecthNote(db, noteList, kbList);
            #endregion

            #region Test Update
            // Test 3
            TestUpdateKnowledgebase(db, kbList);

            // Test 4
            TestUpdateNote(db, noteList, kbList);
            #endregion

            #region TestDelete
            // Test 5
            TestDeleteKnowledgebase(db, kbList);

            // Test 6
            TestDeleteNote(db, kbList);
            #endregion

            // Halt to check result
            Console.Write("press any key to quit: ");
            Console.ReadKey();
        }

        #region TestFetch
        static void TestFetchKnowledgebase(NoteDatabase db, List<KnowledgeBase> kbList)
        {
            #region KnowledgeBase FetchKnowledgeBase(string name)
            foreach (var kb in kbList)
            {
                var actual = db.FetchKnowledgeBase(kb.Name);
                if (actual != null && actual.Name == kb.Name)
                {
                    Console.WriteLine($"kbase {kb.Name}: pass\n");
                }
                else if (actual == null)
                {
                    Console.WriteLine($"ERROR: kbase {kb.Name}, actual is null!\n");
                }
                else
                {
                    Console.WriteLine($"ERROR: kbase {kb.Name}, actual is {actual.Name}!\n");
                }
            }
            #endregion

            #region List<KnowledgeBase> FetchKnowledge()
            var allkb = db.FetchKnowledgeBase();
            if (kbList.Count != allkb.Count)
            {
                Console.WriteLine("different count!");
            }
            for (int i = 0; i < kbList.Count; i++)
            {                
                var kb = kbList[i];
                var actual = i < allkb.Count
                    ? allkb[i]
                    : null;
                if (actual != null && actual.Name == kb.Name)
                {
                    Console.WriteLine($"kbase {kb.Name}: pass\n");
                }
                else if (actual == null)
                {
                    Console.WriteLine($"ERROR: kbase {kb.Name}, actual is null!\n");
                }
                else
                {
                    Console.WriteLine($"ERROR: kbase {kb.Name}, actual is {actual.Name}!\n");
                }
            }
            #endregion

            #region Fetch non-exist KnowledgeBase           
            var shouldBeNull0 = db.FetchKnowledgeBase("");
            var shouldBeNull1 = db.FetchKnowledgeBase(null);
            if (shouldBeNull0 == null && shouldBeNull1 == null)
            {
                Console.WriteLine("Should-be-null: pass");
            }
            else
            {
                Console.WriteLine("Should-be-null: Error");
                Console.ReadKey();
            }


            var shouldBeVoid = db.FetchKnowledgeBase("Java");
            if (shouldBeVoid == KnowledgeBase.VoidKnowledgeBase)
            {
                Console.WriteLine("Should-be-void: pass");
            }
            else
            {
                Console.WriteLine("Should-be-void: Error");
                Console.ReadKey();
            }

            #endregion


        }

        static void TestFecthNote(NoteDatabase db, List<Note> noteList, List<KnowledgeBase> kbList)
        {
            #region FetchNote(directory, title)
            foreach (var note in noteList)
            {
                var actual = db.FetchNote(note.Directory, note.Title);
                if (actual != null
                    && actual.Title == note.Title
                    && actual.Directory == note.Directory
                    )
                {
                    Console.WriteLine($"note {note.Title}: pass\n");
                }
                else if (actual == null)
                {
                    Console.WriteLine($"ERROR: note {note.Title} {note.Directory}, actual is null!\n");
                }
                else
                {
                    Console.WriteLine($"ERROR: note {note.Title} {note.Directory}, (actual) {actual.Title} {actual.Directory}!\n");
                }
            }
            #endregion

            #region FetchNote(directory)
            foreach (var kb in kbList)
            {
                var actual = db.FetchNote(kb.Name);
                var expected = noteList.FindAll(note => note.Directory == kb.Name);
                if (actual.Count  != expected.Count)
                {
                    Console.WriteLine("Error: FetchNote(directory) differs");
                }
                for (int i = 0; i < expected.Count; i++)
                {                    
                    if (i < actual.Count && actual[i] == expected[i])
                    {
                        Console.WriteLine($"note {expected[i].Title}: pass\n");
                    }
                    else if (i >= actual.Count)
                    {
                        Console.WriteLine($"ERROR: note {expected[i].Title} {expected[i].Directory}, actual is null!\n");
                    }
                    else if (actual[i] != expected[i])
                    {
                        Console.WriteLine($"ERROR: note {expected[i].Title} {expected[i].Directory}, (actual) {actual[i].Title} {actual[i].Directory}!\n");
                    }
                }
            }
            #endregion

            #region non-exists            
            var shouldBeNull = db.FetchNote(null);
            if (shouldBeNull == null)
            {
                Console.WriteLine("Note Should-be-null: pass");
            }
            else
            {
                Console.WriteLine("Note Should-be-null: Error");
                Console.ReadKey();
            }

            // Non-exist KnowledgeBase
            var emptyKb = new KnowledgeBase() { Name = "Template" };
            db.addKB(emptyKb);
            var shouldBeEmpty0 = db.FetchNote(emptyKb.Name);
            var shouldBeEmpty1 = db.FetchNote("Java");
            if (shouldBeEmpty0.Count == 0
                && shouldBeEmpty1.Count == 0)
            {
                Console.WriteLine("Note Should-be-empty: pass");
            }
            else
            {
                Console.WriteLine("Note Should-be-empty: Error");
                Console.ReadKey();
            }
            // Restore
            db.DeleteKnowledgeBase(emptyKb.Name);

            // Non-exist note
            var shouldBeVoid = db.FetchNote("Cpp", "tr1");
            if (shouldBeVoid == Note.VoidNote)
            {
                Console.WriteLine("Note Should-be-void: pass");
            }
            else
            {
                Console.WriteLine("Note Should-be-void: Error");
                Console.ReadKey();
            }
            #endregion
        }
        #endregion

        #region TestUpdate
        static void TestUpdateKnowledgebase(NoteDatabase db, List<KnowledgeBase> kbList)
        {
            // New name
            List<string> updated = new List<string>()
            {
                "Mathmatics",
                "Foreign Language",
                "C++"
            };
            
            // Test body
            var toUpdate = db.FetchKnowledgeBase();
            for (int i = 0; i < toUpdate.Count; i++)
            {
                // Update
                toUpdate[i].Name = updated[i];
                toUpdate[i] = db.UpdateKnowledgeBase(toUpdate[i]);

                // Comparison               
                if (toUpdate[i].Name == updated[i])
                {
                    Console.WriteLine($"Update {kbList[i].Name} to {toUpdate[i].Name}");
                }
                else
                {
                    Console.WriteLine($"Update error: {kbList[i].Name} should be {toUpdate[i].Name}");
                }

                // Restore
                db.UpdateKnowledgeBase(kbList[i]);
            }
        }

        static void TestUpdateNote(NoteDatabase db, List<Note> noteList, List<KnowledgeBase> kbList)
        {
            // New contents
            List<string> contents = new List<string>()
            {
                // Math
                "a+b < c",
                "Newton",
                "Linear Transformation",

                // English
                "Clause",
                "What's up",

                // Cpp
                "Don't use not owning raw pointers",
                "lambda, functional, auto",
                "Tempalte template parameters?"
            };

            // Test body
            var toUpdate = new List<Note>();
            foreach (var kb in kbList)
            {
                toUpdate.AddRange(db.FetchNote(kb.Name));
            }
            for (int i = 0; i < toUpdate.Count; i++)
            {
                // Update
                toUpdate[i].Content = contents[i];
                toUpdate[i] = db.UpdateNote(toUpdate[i]);

                // Comparison               
                if (toUpdate[i].Content == contents[i])
                {
                    Console.WriteLine($"Update {noteList[i].Title}: {toUpdate[i].Content}");
                }
                else
                {
                    Console.WriteLine($"Update content: <{toUpdate[i].Content}> should be <{contents[i]}>");
                }

                // Restore
                db.UpdateNote(noteList[i]);
            }
        }
        #endregion

        #region TestDelete
        static void TestDeleteKnowledgebase(NoteDatabase db, List<KnowledgeBase> kbList)
        {
            foreach(var kb in kbList)
            {
                // Delete
                db.DeleteKnowledgeBase(kb.Name);

                // Try to fetch
                var result = db.FetchKnowledgeBase(kb.Name);

                // Check result
                if (result == KnowledgeBase.VoidKnowledgeBase)
                {
                    Console.WriteLine($"Delete {kb.Name}");
                }
                else
                {
                    Console.WriteLine($"Delete failed: {kb.Name}");
                    Console.ReadKey();
                }

                // Restore
                db.addKB(kb);
            }
        }

        static void TestDeleteNote(NoteDatabase db, List<KnowledgeBase> kbList)
        {
            var toDelete = new List<Note>();
            foreach (var kb in kbList)
            {
                toDelete.AddRange(db.FetchNote(kb.Name));
            }
            foreach (var note in toDelete)
            {
                // Delete
                db.DeleteNote(note.Directory, note.Title);

                // Try to fetch
                var result = db.FetchNote(note.Directory, note.Title);

                // Check result
                if (result == Note.VoidNote)
                {
                    Console.WriteLine($"Delete note {note.Title}");
                }
                else
                {
                    Console.WriteLine($"Delete note failed: {note.Title}");
                    Console.ReadKey();
                }

                // Restore
                db.addNote(note);
            }
        }
        #endregion
    }
}
