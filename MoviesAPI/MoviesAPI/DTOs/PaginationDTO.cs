namespace MoviesAPI.DTOs
{
    public class PaginationDTO
    {
        private int page = 1;
        private int recordsPerPage = 10;
        private readonly int maxRecordsPerPage = 50;

        public int Page
        {
            get
            {
                return page;
            }
            set
            {
                page = (value < 1) ? 1 : value;
            }
        }
        public int RecordsPerPage
        {
            get
            {
                return recordsPerPage;
            }
            set
            {
                recordsPerPage = (value > maxRecordsPerPage) ? maxRecordsPerPage : value;
            }
        }

    }
}
