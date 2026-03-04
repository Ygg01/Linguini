list-conjugation-wrong = { -conjugate-red(case: $item) }.
list-conjugation-right = { -conjugate-red(case: 3) }.
-conjugate-red = 
    { $case ->
       *[one] Firefox
        [other] Firefoksie
    }