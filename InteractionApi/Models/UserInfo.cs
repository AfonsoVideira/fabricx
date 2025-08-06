namespace InteractionApi.Models;

public class UserInfo
{
    public string id { get; set; }
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public bool isAdmin { get; set; } = false;
    public bool isActive { get; set; } = true;
} 
