# Structured List

How should I describe to a potential reader what the Structured List is? That question raised in my mind when I was starting to write this article. I think that the best way to describe it is to say something about the background of the problem I had and why I started to search something to solve it. So, let's start with the background.

The usual approach of describing something when you try to understand it is to draw it. We use different kinds of diagrams, we use whiteboards, we use pen and paper, we use screen and pen. And I agree in general, that this approach works quite good. At the same time, the approach has a few disadvantages from my point of view. The primary disadvantage is Drawing is not a fast process. Usual process looks like drawing something on a Whiteboard - the result is something that is hardly readable because of crooked squares and handwriting. How to add this into the task for developers? Em, take a picture of the whiteboard or rewrite it later in a better more understandable way in some tool. The result is we spend time or double time and after a week nobody can easily remember what that box is and why the arrow pointing from this box to that box as there is no information about reasons and our thoughts at the moment of creating. Let's add more text then. Now we have a lot of text and it might be that we better understand the drawing. But it might be that we have a mess of badly aligned and hardly editable and movable elements. To make it look nicer let's spend even more time to align and move elements. Maybe automatic tools can help us? If only they can read our minds and draw all always ideally aligned.

During COVID-19 time, remote teams time after COVID-19, during pair programming sessions I often find myself in a situation when I need to describe something quickly but be able to reuse the output later. The situation was happening when we were sitting in front of our monitors with screen sharing or just in front of one monitor together. 

The other use case, I usually have, is when I am thinking about something and would like to log thinking process, logic, flow, pros and cons, etc. Again, I would like to be able to do it quickly, reuse later in tasks and instructions and so on. 

The problem might be in my instruments. A good instrument can help a lot. So, I started to look for something and think what might help. I use ten fingers blind typing, so during a few pair programming sessions I found useful to open a simple text file, because it is accessible everywhere, and write something using tabs for indentations that indicate sub-thoughts of what is written above and new lines to indicate a new thought. With numbers I was able define steps if required. At this point I started to have following thoughts: I am a huge fan of Markdown for its simplicity and power, I am a huge fan of [Visual Thinking](http://www.visualmess.com/), a lot of really great instruments like D2 diagrams, mermaid js diagrams, terraform are text based, it might be that I should try to reuse my good experience with text files... Many great instruments and approaches are so great because they have simple syntax or convention that make usage very easy. That means that I might just need a simple convention inside of a text file that will allow me to describe my thoughts now and keep them for future usage easily.

Out of that came the idea of Structured List approach. So what is it?

Structured List is a convention of how to describe ideas and thoughts. That's' it. The name might be not the best one, because there are a lot of other meaning of Structured List with different meanings in the Internet, but it describes the core idea.

Structured List is not a silver bullet but a good instrument to be used alone for an appropriate purpose or in combination with other instruments.

P.S. The more or less good drawing instrument that I found much later and that allows to draw something quickly and be able to edit later is [Excalidraw](https://excalidraw.com/). Structured Lists can be combined with it perfectly for short text descriptions.

## The Convention

As name says, Structured List is a list. How is a list usually described in a text or in a markdown document? `-` dash is used in front of some text. So the simple Structure List is just something like:

```
- Thought 1
  - Sub-thought 1
  - Sub-thought 2
- Thought 2
- Thought 3
```

Nothing new an shocking. 

Often when you think about or discuss something you might have something that is questionable or unclear. What symbol do we usually use in case of questions? `?` question mark! Exactly! And we have:

```
- Thought 1
  - Sub-thought 1
  - Sub-thought 2
- Thought 2
  ? Thought to confirm
  ? Point to clarify
- Thought 3
```

I also tend to use kind of clarification titles on top like:

```
- Thought 1
  - Sub-thought 1
  - Sub-thought 2

- Thought 2
  Questions:
  ? Thought to confirm
  
  Assumptions:
  ? Point to clarify

- Thought 3
```


What if we confirmed something right now or later? We can use `!` exclamation mark. As we exclaim `That is true!`.

```
- Thought 1
  - Sub-thought 1
  - Sub-thought 2

- Thought 2
  ? Thought to confirm
  Assumptions:
  ! Point is confirmed by customer

- Thought 3
```

What if an assumption is not confirmed? We can use `-` sign. But wait, we already use it. So, let's use `x` instead of `-`.

```
- Thought 1
  - Sub-thought 1
  - Sub-thought 2

- Thought 2
  ? Thought to confirm
  Assumptions:
  ! Point is confirmed by the customer
  x Not confirmed by the customer

- Thought 3
```

What if I need to describe pros and cons? We can use `+` plus and `-` minus signs. But wait! We already use `-`! Here came the thought that using minus for describing list items might be not a good idea. The solution is either use `*` as in markdown, use `.` as for `statement` or use ... `<nothing>`. Wait what? Remember, I mentioned in the beginning that I am a huge fan of Visual Thinking? Principles of Visual Thinking that are described in the article are Size, Proximity and Alignment. Let's use the ideas.

```
* Thought 1
    * Long sub-thought 1 that might be wrap to the next line. Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.
      Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.

    * Sub-thought 2

* Thought 2
    ? Thought to confirm

    Assumptions:
    ! Point is confirmed by the customer
    x Not confirmed by the customer

* Thought 3
    Pros:
    + Pros 1
    + Pros 2

    Cons:
    - Cons 1
    - Cons 2
```

OR


```
. Thought 1
    . Long sub-thought 1 that might be wrap to the next line. Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.
      Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.

    . Sub-thought 2

. Thought 2
    ? Thought to confirm

    Assumptions:
    ! Point is confirmed by the customer
    x Not confirmed by the customer

. Thought 3
    Pros:
    + Pros 1
    + Pros 2

    Cons:
    - Cons 1
    - Cons 2
```

OR

```
Thought 1
    Long sub-thought 1 that might be wrap to the next line. Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.
    Let make it artificially long to see how it looks like. Let make it artificially long to see how it looks like.
    
    Short Sub-thought 2
    Short Sub-thought 3 can be near each other or split with one line

Thought 2
    ? Thought to confirm
    
    Assumptions:
    ! Point is confirmed by the customer
    x Not confirmed by the customer

Thought 3
    Pros:
    + Pros 1
    + Pros 2

    Cons:
    - Cons 1
    - Cons 2
```

From those three approaches I prefer `*` because it is natively supported by markdown, although sub-items are not supported, and it is easy to understand OR `.` because it is logically clear. The option without any identification at the beginning has lack of visual anchors which makes it harder to read and skip list items.

What other conventional symbols can be used? I rarely use them but why not? Why cannot other symbols be used, especially if they are described in your team documentation and your team knows them? Examples:

```
Process 1:
    Inputs:
    > Input 1
    > Input 2
    > Input 3

    Outputs:
    < Output 1
    < Output 2
    < Output 3

Process 1 implementation:
    To do:
    [x] - done 1
    [x] - done 2
    [ ] - to do 1
    [ ] - to do 2

    Hints:
    - Take in account
    - Take in account 2
```

```
Process 1:
    This 
    & that
    | those

    1. Number 1
    2. Number 2
    3. Number 3

// Is it clear enough what `&`, `|`, `//` are?
// `&` means `and`
// `|` means `or`
// `//` means `comment`, if for whatever reason you need to comment something in a text document
```

```
* Thought 1
    ! Approved by @UserName
    ? Read about #123 in article https://example.com/article.html

// `@` might mean a reference to a person
// `#` might mean a reference to a something other than a person
```

There are a lot of single character symbols are left and the can be used in your team convention. But apart from that, words can be used as they were used in examples above.
Words that I usually uses are: `Assumptions`, `Questions`, `Cons`, `Pros`, `Inputs`, `Outputs`, `Process`, `Steps`, `Comments`. I also use words that are usually used as a reserved words in programming languages: `if`, `else`, `while`, etc. They are especially useful if you are trying to describe an abstract algorithm or a process.

## Instead of a conclusion

Having in your arsenal useful instruments is a great thing. Structured Lists give you an additional, simple, powerful, extensible, maybe situational instrument to achieve your goals. So use it if you think it is worth it. It doesn't need any special training or tool support. Just use obvious and self-explanatory convention and you are good to go.
