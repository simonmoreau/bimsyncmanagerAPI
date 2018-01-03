namespace bimsyncManagerAPI.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string bimsync_id {get;set;}
        public string PowerBiSecret{get;set;}
        public string AccessToken{get;set;}
        public string TokenType{get;set;}
        public int? TokenExpireIn{get;set;}
        public string RefreshToken{get;set;}
        public string RefreshDate{get;set;}
        public string BCFToken{get;set;}


    }
}

// (create line with user_id | power_bi_secret | token.access_token | token.token_type
// | token.expires_in | token.refresh_token | token_refresh_date)

