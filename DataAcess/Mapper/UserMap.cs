using Dapper.FluentMap.Mapping;
using DataAccess.Common.Entity;

namespace DataAcess.Mapper;
//NOT USED ANYMORE. JUST HERE FOR FUTURE REFERENCE.
public class UserMap : EntityMap<UserEntity>
{
    public UserMap()
    {
        Map(x => x.Id).ToColumn("id");
        Map(x => x.Name).ToColumn("name");
        Map(x => x.Email).ToColumn("email");
        Map(x => x.CreatedAt).ToColumn("created_at");
        Map(x => x.GoogleSubject).ToColumn("google_subject");
        Map(x => x.GoogleSignInEnabled).ToColumn("google_sign_in_enabled");
    }
}