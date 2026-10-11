import { useState } from 'react'
import AddIcon from '@mui/icons-material/Add'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import { useCreateTask } from '../api/tasks'
import { useNotifications } from '../notifications/useNotifications'

const MAX_TITLE_LENGTH = 200

export default function NewTaskForm() {
  const [title, setTitle] = useState('')
  const createTask = useCreateTask()
  const { notifySuccess, notifyError } = useNotifications()

  const trimmedTitle = title.trim()
  const tooLong = trimmedTitle.length > MAX_TITLE_LENGTH

  function handleSubmit(event: React.SyntheticEvent) {
    event.preventDefault()
    if (!trimmedTitle || tooLong) {
      return
    }

    createTask.mutate(
      { title: trimmedTitle },
      {
        onSuccess: () => {
          setTitle('')
          notifySuccess('Task added MODIF TEST.')
        },
        onError: (error) => notifyError(`Could not add the task: ${error.message}`),
      },
    )
  }

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', gap: 1 }}>
      <TextField
        fullWidth
        size="small"
        label="New task"
        value={title}
        onChange={(event) => setTitle(event.target.value)}
        error={tooLong}
        helperText={tooLong ? `Keep it under ${MAX_TITLE_LENGTH} characters.` : ' '}
        slotProps={{ htmlInput: { 'aria-label': 'New task' } }}
      />
      <Button
        type="submit"
        variant="contained"
        startIcon={<AddIcon />}
        disabled={!trimmedTitle || tooLong || createTask.isPending}
        sx={{ height: 40, flexShrink: 0 }}
      >
        Add
      </Button>
    </Box>
  )
}
