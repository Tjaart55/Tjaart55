class State:
    Z = 0
    SecondOne = 1
    Non = 2


def Solution0(N):
    s = format(N, "b")
    z = 0
    maxval = 0

    state = State.Non

    for c in s:
        if state == State.Non and c == '1':
            state = State.Z
        elif state == State.Z and c == '0':
            z += 1
        elif state == State.Z and c == '1':
            if z > maxval:
                maxval = z

                z = 0
                state = State.Z

    return maxval


a = Solution0(133)

