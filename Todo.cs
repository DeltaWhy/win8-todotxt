using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TodoTxt
{
    class Todo
    {
        public string Text { get; set; }
        public bool Completed { get; set; }
        public DateTime CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
        public List<string> Projects { get; set; }
        public List<string> Contexts { get; set; }
        public Dictionary<string, string> Meta { get; set; }

        public Todo(string line)
        {
            Match match = Regex.Match(line, @"((?<completed>x )(?<completedate>\d{4}-\d{2}-\d{2} )?)?"+
                @"(\((?<priority>[A-Z])\) )?" +
                @"(?<createdate>\d{4}-\d{2}-\d{2} )?" +
                @"(?<text>.*)");
            this.Text = match.Groups["text"].Value;
            this.Completed = (match.Groups["completed"].Value == "x ");
            this.Priority = match.Groups["priority"].Value;
            try
            {
                this.CompletedDate = DateTime.Parse(match.Groups["completedate"].Value);
            } catch (FormatException e) { }
            try
            {
                this.CreatedDate = DateTime.Parse(match.Groups["createdate"].Value);
            } catch (FormatException e) { }

            this.Projects = (from Match m in Regex.Matches(this.Text, @"\+(\S+)") select m.Groups[1].Value).ToList();
            this.Contexts = (from Match m in Regex.Matches(this.Text, @"@(\S+)") select m.Groups[1].Value).ToList();
            this.Meta = new Dictionary<string,string>();
            foreach (Match m in Regex.Matches(this.Text, @"([^\s:]+):([^\s:]+)")) {
                this.Meta[m.Groups[1].Value.ToLower()] = m.Groups[2].Value;
            }

            try
            {
                this.DueDate = DateTime.Parse(this.Meta["due"]);
            }
            catch (FormatException e) { }
            catch (KeyNotFoundException e) { }
        }

        public Style TextStyle
        {
            get
            {
                return (this.Completed ? (Style)Application.Current.Resources["CompletedTodoTextStyle"] : (Style)Application.Current.Resources["TodoTextStyle"]);
            }
        }
        public override string ToString()
        {
            return (this.Completed ? "x " : "") +
                (this.CompletedDate != DateTime.MinValue ? this.CompletedDate.ToString("yyyy-MM-dd ") : "") +
                (this.Priority != null && this.Priority != "" ? "(" + this.Priority + ") " : "") +
                (this.CreatedDate != DateTime.MinValue ? this.CreatedDate.ToString("yyyy-MM-dd ") : "") +
                this.Text;
        }
    }

    class TodoList
    {
        public async static Task<IEnumerable<Todo>> GetAsyncFromFile(StorageFile file)
        {
            IList<string> lines = await FileIO.ReadLinesAsync(file);
            return from line in lines select new Todo(line);
        }
    }
}
