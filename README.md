### todotxtconsole

Todo.txt Console is an implemention of [todo.txt](http://todotxt.com/) using the .NET framework, and originating from benrhughes' [todotxt.net](https://github.com/benrhughes/todotxt.net). As far as I am aware, it is compliant with [Gina's spec](https://github.com/ginatrapani/todo.txt-cli/wiki/The-Todo.txt-Format),
with one exception: although tasks can be entered in any format supported by the spec, tasks are always written to file in a consistent format. 

#### Goals

 - Console-based interface
 - Improved speed and efficiency over todo.sh
 - Extendable command design
 - Compliance with Gina's specs

#### Current features:

 - Fast!
 - Sorting and grouping capabilities when listing tasks
 - Remembers preferences for the todo.txt file, archive file, sort order, and group order
