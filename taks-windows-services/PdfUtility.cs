using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;

namespace taks_windows_services
{
	public class PdfUtility
	{
		private readonly Section _documentSection;
		private readonly Document _document;
		private readonly PdfDocumentRenderer _renderer;

		public PdfUtility()
		{
			_documentSection = _document.AddSection();
			_document = new Document();
			_renderer = new PdfDocumentRenderer();
		}

		public void AddImage(string path)
		{
			Image image = _documentSection.AddImage(path);

			image.Height = _document.DefaultPageSetup.PageHeight;
			image.Width = _document.DefaultPageSetup.PageWidth;
			image.ScaleHeight = 0.7;
			image.ScaleWidth = 0.7;

			_documentSection.AddPageBreak();
		}

		public void Save(string path)
		{
			_renderer.Document = _document;
			_renderer.RenderDocument();
			_renderer.Save(path);
		}
	}
}
