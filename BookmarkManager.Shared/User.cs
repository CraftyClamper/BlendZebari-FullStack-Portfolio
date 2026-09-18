using System;
using System.Collections.Generic;
using System.Text;

namespace BookmarkManager.Shared
{
    public class User
    {
        //The ID belonging to every user
        public int Id { get; set; }

        //The username belonging to each user belonging to the ID
        public string Username { get; set; } = string.Empty;

        //Hashed password for the user to be able to log in
        public string PasswordHash { get; set; } = string.Empty;


        //Local pin/password for the Chrome extension Lock feature
        public string ExtensionPinHash {  get; set; } = string.Empty;

    }
}
