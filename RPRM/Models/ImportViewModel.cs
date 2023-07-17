namespace RPRM.Models
{
    public class ImportViewModel
    {
        public string ImportType { get; set; }
        public IFormFile File { get; set; }
        public int SelectedSheetIndex { get; set; }

    }
}
