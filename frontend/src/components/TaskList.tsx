import { useEffect } from 'react'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import List from '@mui/material/List'
import Typography from '@mui/material/Typography'
import { useTasks } from '../api/tasks'
import { useNotifications } from '../notifications/useNotifications'
import TaskListItem from './TaskListItem'

export default function TaskList() {
  const { data: tasks, isPending, isError, error } = useTasks()
  const { notifyError } = useNotifications()

  useEffect(() => {
    if (isError) {
      notifyError(`Could not load the tasks: ${error.message}`)
    }
  }, [isError, notifyError])

  if (isPending) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress aria-label="Loading tasks" />
      </Box>
    )
  }

  if (isError) {
    return <Alert severity="error">Could not load the tasks: {error.message}</Alert>
  }

  if (tasks.length === 0) {
    return (
      <Typography sx={{ py: 4, textAlign: 'center' }} color="text.secondary">
        Nothing to do yet. Add your first task above.
      </Typography>
    )
  }

  return (
    <List disablePadding>
      {tasks.map((task, index) => (
        <Box key={task.id}>
          {index > 0 && <Divider component="li" />}
          <TaskListItem task={task} />
        </Box>
      ))}
    </List>
  )
}
