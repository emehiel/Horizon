# Copyright (c) 2022-2023 Scott Plantenga


# code graveyard for code that is no longer used, but may be useful later

def secantMinimizer(x0, x1, f, k, tol):
    '''
    Secant Method single variable minimization
    '''
    (g_n_minus_1, f_n_minus_1) = fprime(f, x0, k)
    (g_n, f_n) = fprime(f, x1, k)

    magFprime = min(abs(g_n), abs(g_n_minus_1))
    errX = abs(g_n - g_n_minus_1)
    err = min(magFprime, errX)

    x_n_minus_1 = x0
    x_n = x1

    ii = 0
    while (err > tol):
        # compute next step
        x_n_plus_1 = x_n - g_n * (x_n - x_n_minus_1) / (g_n - g_n_minus_1)

        # update variables
        x_n_minus_1 = x_n
        x_n = x_n_plus_1
        g_n_minus_1 = g_n
        (g_n, f_n) = fprime(f, x_n, k)

        magFprime = abs(g_n)
        errX = abs(g_n - g_n_minus_1)
        err = min(magFprime, errX)
        ii += 1

    return (x_n, ii, f_n, magFprime, errX)


def solveForMinDV_Secant(RV0, RVf, n, tol):
    '''
    generate initial guess and call secant method
    '''

    # Initial Guess is n*t = pi/2
    t0 = math.pi / (n * 2)
    t1 = t0 * 1.01

    k = (RV0, RVf, n) # tuple of constants
    (tStar, numIts, cost, magDeriv, magStep) = secantMinimizer(t0, t1, impCost, k, tol)
    tPiRatio = tStar * n / math.pi
    # print('\ttStar = ' + str(tStar) + ' (' + str(tPiRatio) + '*pi), ' + str(numIts) + ' iterations, cost (f) = ' + str(cost) + ', fprime = ' + str(magDeriv) + ', final step size = ' + str(magStep))
    return tStar

