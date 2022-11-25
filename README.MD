# Quera Grade Extractor

Extract Quera grades from Quera's excel output and make them storable in spreadsheets. It also extracts delay hours from
Quera if delay penalty is linear.

## Preparations

### Delay

If you want to extract delay hours as well, choose a linear penalty formula in Quera. For example for 48 hour delay use
this formula:

```
2d 100-(100*delay/(60*60*24*2))
```

Currently the delay amount is hardcoded in the code. Later I will add a more flexible method for handling it.

### Student List

A file named `students.txt` must exist next to the executable. This file must contain student IDs in each line of it.
The output file contains the student ID's in this list in the order specified in `students.txt`.

### Quera's Excel Report

Quera's excel report for an assignment must exist under the name of `quera.xlsx` next to the executable.

## Running

Just open the program. It will do the conversion.

The output will be available in `grades.xlsx`.