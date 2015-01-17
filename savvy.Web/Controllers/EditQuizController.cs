﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using savvy.Data;
using savvy.Web.Models;

namespace savvy.Web.Controllers
{
    public class EditQuizController : BaseController
    {
        public EditQuizController(ISavvyRepository repo) : base(repo)
        {
        }

        public List<EditQuizModel> Get()
        {
            return Repository.GetAllQuizzes().Select(quiz => ModelFactory.Edit.Create(quiz)).ToList();
        }

        public IHttpActionResult Get(int id)
        {
            var quiz = Repository.GetQuiz(id);

            if (quiz == null)
            {
                return NotFound();
            }

            return Ok(ModelFactory.Edit.Create(quiz));
        }
    }
}
