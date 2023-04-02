
Technologies:
-Minimal API
-SqLite
-EntityFramework Core

Steps to test the program
1- Run the app.
2- To test the URL shortener feature use the "url_shortener" endpoint. "url" parameter must be a valid URL otherwise the program will return bad request (code 400).
If input url is valid, program will return a shortened version of your input.
3- To test the URL customizer feature use the "url_customizer" endpoint. "baseUrl" parameter must be a valid URL and custom url chunk must be a unique value otherwise 
the program will return bad request (code 400). If input url is valid, program will return a customized version of your input.

For testing you can use both SwaggerUI and Postman.
