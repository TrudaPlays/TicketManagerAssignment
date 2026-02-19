I encountered this challenge when writing the I/O for the ticket manager. When adding a path to save the tickets in a csv file, if the user stopped at a folder
and didn't specify the file name it would throw an error and not save the file. While it didn't break the program, I didn't like how it non-user-friendly crashed it
so to speak. So I added some more messages to the console that outlined what kind of path the program is expecting. I found that it looked much nicer
and more streamlined and gave a more user-friendly experience by telling what went wrong and what the program expects, instead of just blankly ending the session.
The error handling was essential in this coding because I had to used try and catch to catch the different exceptions and print according messages to the console.
