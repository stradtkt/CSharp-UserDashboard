using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UserDashboard.Models
{
    public class User : BaseEntity
    {
        [Key]
        public int user_id {get;set;}
        public string first_name {get;set;}
        public string last_name {get;set;}
        public string email {get;set;}
        public string password {get;set;}
        public List<Message> Messages {get;set;}

        public User()
        {
            Messages = new List<Message>();
            created_at = DateTime.Now;
            updated_at = DateTime.Now;
        }
    }   
}