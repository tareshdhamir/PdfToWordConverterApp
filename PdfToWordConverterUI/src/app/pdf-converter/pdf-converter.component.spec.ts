import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { PdfConverterComponent } from './pdf-converter.component';
import { provideHttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

describe('PdfConverterComponent', () => {
  let component: PdfConverterComponent;
  let fixture: ComponentFixture<PdfConverterComponent>;
  let httpMock: HttpTestingController;
  let apiUrl = environment.apiUrl;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PdfConverterComponent], // Import the standalone component
      providers: [
        provideHttpClient(), // Provide HttpClient
        provideHttpClientTesting() // Provide HttpClientTesting for mocking HTTP requests
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PdfConverterComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController); // Inject HttpTestingController
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify(); // Verify that there are no outstanding HTTP requests after each test
  });

  it('should upload the file and download the converted file', () => {
    const mockFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.selectedFile = mockFile;

    // Call the method to trigger the HTTP request
    component.onUpload();

    // Expect the request with the correct URL and method
    const req = httpMock.expectOne(`${apiUrl}/PdfToWord/convert`);
    expect(req.request.method).toBe('POST');

    // Simulate a Blob response, which is what the component expects from the API
    const blob = new Blob(['test content'], { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
    req.flush(blob);  // Pass the blob in the flush method

    expect(component.resultMessage).toBe('Conversion Successful!');
  });

  it('should show an error if no file is selected', () => {
    component.selectedFile = null;  // No file selected
    component.onUpload();
    expect(component.resultMessage).toBe('Please select a file first!');
  });

  it('should handle HTTP error response', () => {
    const mockFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.selectedFile = mockFile;

    component.onUpload();

    const req = httpMock.expectOne(`${apiUrl}/PdfToWord/convert`);
    
    // Simulate a Blob response for the error, as the responseType is 'blob'
    const errorBlob = new Blob(['Error occurred'], { type: 'text/plain' });
    req.flush(errorBlob, { status: 500, statusText: 'Server Error' });

    expect(component.resultMessage).toBe('Conversion Failed!');
  });

  it('should handle empty or invalid response Blob', () => {
    const mockFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.selectedFile = mockFile;

    component.onUpload();

    const req = httpMock.expectOne(`${apiUrl}/PdfToWord/convert`);
    
    // Simulate an empty Blob response
    const emptyBlob = new Blob([], { type: 'application/pdf' });
    req.flush(emptyBlob);

    expect(component.resultMessage).toBe('File conversion failed. Empty or invalid response.');
  });

  it('should reset file and message', () => {
    component.selectedFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.resultMessage = 'Some message';

    component.resetFile();

    expect(component.selectedFile).toBeNull();
    expect(component.resultMessage).toBe('');
  });

  it('should trigger file download when conversion is successful', () => {
    const mockFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.selectedFile = mockFile;
  
    // Mock the anchor element and spy on the click method
    const anchorMock = document.createElement('a');
    const clickSpy = spyOn(anchorMock, 'click').and.callFake(() => {});
  
    // Spy on document.createElement to return the mock anchor
    spyOn(document, 'createElement').and.returnValue(anchorMock);
  
    // Call the method to trigger the HTTP request
    component.onUpload();
  
    // Expect the request with the correct URL and method
    const req = httpMock.expectOne(`${apiUrl}/PdfToWord/convert`);
    expect(req.request.method).toBe('POST');
  
    // Simulate a Blob response, which is what the component expects from the API
    const blob = new Blob(['test content'], { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });
    req.flush(blob);  // Pass the blob in the flush method
  
    expect(component.resultMessage).toBe('Conversion Successful!');
  
    // Verify that the click method on the anchor element was called
    expect(clickSpy).toHaveBeenCalled();
  });  

  it('should handle network error during file conversion', () => {
    const mockFile = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    component.selectedFile = mockFile;

    component.onUpload();

    const req = httpMock.expectOne(`${apiUrl}/PdfToWord/convert`);
    
    // Simulate an HTTP error response
    req.error(new ErrorEvent('Network error'), { status: 500, statusText: 'Internal Server Error' });

    expect(component.resultMessage).toBe('Conversion Failed!');
  });
});
