using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestPostWithRazor.Pages
{
    public class IndexModel : PageModel
    {
        public string Message
        {
            get;
            set;
        }

        public void OnGet()
        {
            Message = "Enter your message here";
        }

        public void OnPost()
        {
            Message = Request.Form[nameof(Message)];
        }
    }
}