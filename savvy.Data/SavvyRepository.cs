using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using savvy.Data.Entities;
using savvy.Data.Entities.Questions;
using savvy.Data.Extensions;
using savvy.Data.Util;

namespace savvy.Data
{
    public class SavvyRepository : ISavvyRepository
    {
        #region Private Properties

        private SavvyContext _ctx;

        #endregion

        #region Constructor

        public SavvyRepository(SavvyContext ctx)
        {
            _ctx = ctx;
        }

        #endregion

        #region General

        public void SaveChanges()
        {
            _ctx.SaveChanges();
        }

        // Helper to update objects in context
        protected void UpdateEntity<T>(DbSet<T> dbSet, T entity) where T : class
        {
            dbSet.AttachAsModified(entity, _ctx);
        }

        #endregion

        #region Quizzes

        public Quiz GetQuiz(int quizId)
        {
            var quiz = _ctx.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.QuizId == quizId && q.Deleted == false);

            if (quiz == null)
            {
                return null;
            }

            quiz.Questions.Sort((q1, q2) => q1.SequenceNum.CompareTo(q2.SequenceNum));

            foreach (var question in quiz.Questions)
            {
                LoadQuestion(question);
            }

            return quiz;
        }

        public List<Quiz> GetAllQuizzes()
        {
            List<Quiz> quizzes = _ctx.Quizzes
                .Include(quiz => quiz.Questions)
                .Where(q => q.Deleted == false)
                .ToList();

            foreach (var quiz in quizzes)
            {
                quiz.Questions.Sort((q1, q2) => q1.SequenceNum.CompareTo(q2.SequenceNum));

                foreach (var question in quiz.Questions)
                {
                    LoadQuestion(question);
                }
            }

            return quizzes;
        }

        public bool CreateQuiz(Quiz quiz)
        {
            _ctx.Quizzes.Add(quiz);
            return _ctx.SaveChanges() > 0;
        }

        public bool UpdateQuiz(Quiz quiz)
        {
            _ctx.Quizzes.AttachAsModified(quiz, _ctx);
            return _ctx.SaveChanges() > 0;
        }

        public bool DeleteQuiz(int quizId, bool permanent = false)
        {
            var quiz = _ctx.Quizzes.First(q => q.QuizId == quizId);

            if (permanent)
            {
                _ctx.Quizzes.Remove(quiz);
            }
            else
            {
                quiz.Deleted = true;
            }

            return _ctx.SaveChanges() > 0;
        }

        #endregion

        #region Questions

        public Question GetQuestion(int questionId)
        {
            var question = _ctx.Questions.FirstOrDefault(q => q.QuestionId == questionId);

            if (question == null)
            {
                return null;
            }

            LoadQuestion(question);

            return question;
        }

        public Question GetQuestion(int quizId, int sequenceNum)
        {
            var question = _ctx.Questions.FirstOrDefault(q => q.QuizId == quizId && q.SequenceNum == sequenceNum);

            if (question == null)
            {
                return null;
            }

            LoadQuestion(question);

            return question;
        }

        /// <summary>
        /// Fully loads all properties for the given question.
        /// </summary>
        /// <param name="question">Partially-loaded question.</param>
        /// <remarks>
        /// Entity Framework does not have a good way to eager load navigation properties of derived classes when
        /// using Table Per Type inheritance. This method is a work around to load any additional properties that
        /// we may need for all the different kinds of questions (for example, loading all the choices for a
        /// multiple choice question).
        /// 
        /// See:
        /// https://connect.microsoft.com/VisualStudio/feedback/details/594289/in-entity-framework-there-should-be-a-way-to-eager-load-include-navigation-properties-of-a-derived-class
        /// </remarks>
        public void LoadQuestion(Question question)
        {
            var mc = question as MultipleChoiceQuestion;
            if (mc != null && mc.Choices == null)
            {
                mc.Choices = _ctx.Choices.Where(c => c.QuestionId == question.QuestionId).ToList();
            }
        }

        public bool CreateQuestion(Question question)
        {
            _ctx.Questions.Add(question);
            return _ctx.SaveChanges() > 0;
        }

        public bool DeleteQuestion(int questionId)
        {
            var question = _ctx.Questions.First(q => q.QuestionId == questionId);
            _ctx.Questions.Remove(question);
            return _ctx.SaveChanges() > 0;
        }

        #endregion
    }
}
